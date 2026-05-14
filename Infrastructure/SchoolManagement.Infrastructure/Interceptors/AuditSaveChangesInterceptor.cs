using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SchoolManagement.Infrastructure.Interceptors
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IUserSessionService _userSessionService;

        public AuditSaveChangesInterceptor(
            IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            DbContext? context = eventData.Context;

            if (context == null)
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            List<AuditLog> logs = [];

            foreach (EntityEntry entry in context.ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                {
                    string? displayName = (entry.Entity as IAuditableEntity)?.GetAuditName();
                    int? currentUserId = _userSessionService.CurrentUser?.Id;

                    AuditLog log = new AuditLog()
                    {
                        Action = entry.State.ToString(),
                        EntityType = entry.Entity.GetType().Name,
                        EntityName = displayName,
                        Timestamp = DateTime.UtcNow,
                        UserId = currentUserId
                    };

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            log.NewValues = entry.CurrentValues.SerializeProperties();
                            break;
                        case EntityState.Modified:
                            if (string.IsNullOrWhiteSpace(log.OldValues))
                                break;
                            log.OldValues = JsonDataHelper.SerializeModifiedProperties(
                                entry.OriginalValues, entry.CurrentValues, true);
                            log.NewValues = JsonDataHelper.SerializeModifiedProperties(
                                entry.CurrentValues, entry.OriginalValues, true);
                            break;
                        case EntityState.Deleted:
                            log.OldValues = entry.OriginalValues.SerializeProperties();
                            break;
                    }

                    logs.Add(log);
                }
            }

            if (logs.Count > 0)
            {
                await context.Set<AuditLog>().AddRangeAsync(logs, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}

