using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class ClassRepository : IClassRepository
    {
        private readonly SchoolDbContext _context;

        public ClassRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Class>> GetAllAsync()
        {
            return await _context.Classes.ToListAsync();
        }

        public async Task<Class?> GetByIdAsync(int id)
        {
            return await _context.Classes.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Class @class)
        {
            ArgumentNullException.ThrowIfNull(@class);
            await _context.Classes.AddAsync(@class);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Class @class)
        {
            ArgumentNullException.ThrowIfNull(@class);
            var existing = await _context.Classes.FindAsync(@class.Id);
            if (existing == null)
            {
                _context.Classes.Attach(@class);
                _context.Entry(@class).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(@class);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Class @class)
        {
            ArgumentNullException.ThrowIfNull(@class);
            _context.Classes.Remove(@class);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Class>> FindAsync(Expression<Func<Class, bool>> predicate, int? page, int pageSize, Func<IQueryable<Class>, IOrderedQueryable<Class>>? orderBy = null, params Expression<Func<Class, object>>[] includes)
        {
            IQueryable<Class> query = _context.Classes;

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
