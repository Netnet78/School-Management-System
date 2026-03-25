using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetAllAsync();
        Task<Permission?> GetByIdAsync(int id);
        Task AddAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task DeleteAsync(Permission permission);
        Task SaveAsync();
    }

    public class PermissionRepository : IPermissionRepository
    {
        private readonly SchoolDbContext _context;

        public PermissionRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Permission permission)
        {
            ArgumentNullException.ThrowIfNull(permission);
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Permission permission)
        {
            ArgumentNullException.ThrowIfNull(permission);
            var existing = await _context.Permissions.FindAsync(permission.Id);
            if (existing == null)
            {
                _context.Permissions.Attach(permission);
                _context.Entry(permission).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(permission);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Permission permission)
        {
            ArgumentNullException.ThrowIfNull(permission);
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}