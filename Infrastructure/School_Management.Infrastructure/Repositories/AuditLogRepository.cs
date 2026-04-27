using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly SchoolDbContext _context;

        public AuditLogRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs.ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            ArgumentNullException.ThrowIfNull(auditLog);
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AuditLog auditLog)
        {
            ArgumentNullException.ThrowIfNull(auditLog);
            var existing = await _context.AuditLogs.FindAsync(auditLog.Id);
            if (existing == null)
            {
                _context.AuditLogs.Attach(auditLog);
                _context.Entry(auditLog).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(auditLog);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AuditLog auditLog)
        {
            ArgumentNullException.ThrowIfNull(auditLog);
            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> FindAsync(Expression<Func<AuditLog, bool>> predicate, int? page, int pageSize, Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>? orderBy = null, params Expression<Func<AuditLog, object>>[] includes)
        {
            IQueryable<AuditLog> query = _context.AuditLogs;

            // Apply includes first
            foreach (var include in includes)
                query = query.Include(include);

            // Apply filter
            query = query.Where(predicate);

            // Apply sorting
            if (orderBy != null)
                query = orderBy(query);

            return await query
                .Skip((pageSize * (page - 1)) ?? pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
