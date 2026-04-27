using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly SchoolDbContext _context;

        public DepartmentRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);
            var existing = await _context.Departments.FindAsync(department.Id);
            if (existing == null)
            {
                _context.Departments.Attach(department);
                _context.Entry(department).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(department);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Department>> FindAsync(Expression<Func<Department, bool>> predicate, int? page, int pageSize, Func<IQueryable<Department>, IOrderedQueryable<Department>>? orderBy = null, params Expression<Func<Department, object>>[] includes)
        {
            IQueryable<Department> query = _context.Departments;

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
