using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class GradeRepository : IGradeRepository
    {
        private readonly SchoolDbContext _context;

        public GradeRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Grade>> GetAllAsync()
        {
            return await _context.Grades.ToListAsync();
        }

        public async Task<Grade?> GetByIdAsync(int id)
        {
            return await _context.Grades.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddAsync(Grade grade)
        {
            ArgumentNullException.ThrowIfNull(grade);
            await _context.Grades.AddAsync(grade);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Grade grade)
        {
            ArgumentNullException.ThrowIfNull(grade);
            var existing = await _context.Grades.FindAsync(grade.Id);
            if (existing == null)
            {
                _context.Grades.Attach(grade);
                _context.Entry(grade).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(grade);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Grade grade)
        {
            ArgumentNullException.ThrowIfNull(grade);
            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Grade>> FindAsync(Expression<Func<Grade, bool>> predicate, int? page, int pageSize, Func<IQueryable<Grade>, IOrderedQueryable<Grade>>? orderBy = null, params Expression<Func<Grade, object>>[] includes)
        {
            IQueryable<Grade> query = _context.Grades;

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
