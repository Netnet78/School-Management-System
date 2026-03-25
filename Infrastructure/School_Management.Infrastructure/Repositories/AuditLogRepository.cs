using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(int id);
        Task AddAsync(AuditLog auditLog);
        Task UpdateAsync(AuditLog auditLog);
        Task DeleteAsync(AuditLog auditLog);
        Task SaveAsync();
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly SchoolDbContext _context;

        public AuditLogRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetAllAsync()
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
    }
}