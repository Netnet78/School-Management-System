using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class PermissionRepository : IPermissionRepository
    {
        private readonly SchoolDbContext _context;

        public PermissionRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
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

        public async Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate, int? page, int pageSize, Func<IQueryable<Permission>, IOrderedQueryable<Permission>>? orderBy = null, params Expression<Func<Permission, object>>[] includes)
        {
            IQueryable<Permission> query = _context.Permissions;

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
