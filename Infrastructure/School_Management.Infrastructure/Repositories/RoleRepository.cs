using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class RoleRepository : IRoleRepository
    {
        private readonly SchoolDbContext _context;

        public RoleRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task AddAsync(Role role)
        {
            ArgumentNullException.ThrowIfNull(role);
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            ArgumentNullException.ThrowIfNull(role);
            var existing = await _context.Roles.FindAsync(role.Id);
            if (existing == null)
            {
                _context.Roles.Attach(role);
                _context.Entry(role).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(role);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Role role)
        {
            ArgumentNullException.ThrowIfNull(role);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> FindAsync(Expression<Func<Role, bool>> predicate, int? page, int pageSize, Func<IQueryable<Role>, IOrderedQueryable<Role>>? orderBy = null, params Expression<Func<Role, object>>[] includes)
        {
            IQueryable<Role> query = _context.Roles;

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
