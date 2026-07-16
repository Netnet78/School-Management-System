using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Features.AuditLogs.Enums;
using System.ComponentModel;
using SchoolManagement.Infrastructure.Shared.Data;

namespace SchoolManagement.Infrastructure.Interceptors
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;

        public AuditSaveChangesInterceptor(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            DbContext? context = eventData.Context;

            if (context == null)
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            IUserSessionService? userSessionService = _serviceProvider.GetService<IUserSessionService>();
            List<AuditLog> logs = [];

            foreach (EntityEntry entry in context.ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                    continue;

                Type entityType = entry.Entity.GetType();

                AuditIgnoreTypeAttribute? ignoreAttr = entityType
                    .GetCustomAttributes(typeof(AuditIgnoreTypeAttribute), inherit: true)
                    .Cast<AuditIgnoreTypeAttribute>()
                    .FirstOrDefault();

                string? entityTypeName = entityType.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (string.IsNullOrWhiteSpace(entityTypeName))
                {
                    if (entityType.IsGenericType && entityType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        entityTypeName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
                    }
                    else
                    {
                        entityTypeName = entityType.Name;
                    }
                }

                if (ignoreAttr?.Operation.HasFlag(AuditOperation.All) == true)
                    continue;

                AuditOperation operation = entry.State switch
                {
                    EntityState.Added => AuditOperation.Insert,
                    EntityState.Modified => AuditOperation.Update,
                    EntityState.Deleted => AuditOperation.Delete,
                    _ => AuditOperation.None,
                };

                if (ignoreAttr != null && !ignoreAttr.Operation.HasFlag(operation))
                    continue;

                string? displayName = (entry.Entity as IAuditableEntity)?.CustomAuditName();
                string? customDescription = (entry.Entity as IAuditableEntity)?.CustomAuditDescription();

                int? currentUserId = userSessionService?.CurrentUser?.Id;

                AuditLog log = new()
                {
                    Action = entry.State switch
                    {
                        EntityState.Added => "Inserted",
                        EntityState.Modified => "Updated",
                        EntityState.Deleted => "Deleted",
                        _ => entry.State.ToString(),
                    },
                    EntityType = entityTypeName,
                    EntityName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName,
                    Timestamp = DateTime.UtcNow,
                    UserId = currentUserId,
                    CustomDescription = string.IsNullOrWhiteSpace(customDescription) ? string.Empty : customDescription,
                };

                switch (entry.State)
                {
                    case EntityState.Modified:
                        bool allHidden = entry.Properties
                            .Where(p => p.IsModified)
                            .Select(p => p.Metadata.PropertyInfo)
                            .All(pi => pi?.IsDefined(typeof(AuditIgnoreAttribute), inherit: true) == true);

                        if (allHidden)
                            continue;

                        log.OldValues = JsonDataHelper.SerializeModifiedProperties(
                            entry.OriginalValues, entry.CurrentValues, true);
                        log.NewValues = JsonDataHelper.SerializeModifiedProperties(
                            entry.CurrentValues, entry.OriginalValues, true);
                        break;
                    case EntityState.Added:
                        log.NewValues = entry.CurrentValues.SerializeProperties();
                        break;
                    case EntityState.Deleted:
                        log.OldValues = entry.OriginalValues.SerializeProperties();
                        break;
                }

                logs.Add(log);
            }

            if (logs.Count > 0)
            {
                await context.Set<AuditLog>().AddRangeAsync(logs, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
