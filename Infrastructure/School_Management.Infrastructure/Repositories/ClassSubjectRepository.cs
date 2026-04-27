using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class ClassSubjectRepository : IClassSubjectRepository
    {
        private readonly SchoolDbContext _context;

        public ClassSubjectRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassSubject>> GetAllAsync()
        {
            return await _context.ClassSubjects.ToListAsync();
        }

        public async Task<ClassSubject?> GetByIdAsync(int id)
        {
            return await _context.ClassSubjects.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(ClassSubject classSubject)
        {
            ArgumentNullException.ThrowIfNull(classSubject);
            await _context.ClassSubjects.AddAsync(classSubject);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ClassSubject classSubject)
        {
            ArgumentNullException.ThrowIfNull(classSubject);
            var existing = await _context.ClassSubjects.FindAsync(classSubject.Id);
            if (existing == null)
            {
                _context.ClassSubjects.Attach(classSubject);
                _context.Entry(classSubject).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(classSubject);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ClassSubject classSubject)
        {
            ArgumentNullException.ThrowIfNull(classSubject);
            _context.ClassSubjects.Remove(classSubject);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ClassSubject>> FindAsync(Expression<Func<ClassSubject, bool>> predicate, int? page, int pageSize, Func<IQueryable<ClassSubject>, IOrderedQueryable<ClassSubject>>? orderBy = null, params Expression<Func<ClassSubject, object>>[] includes)
        {
            IQueryable<ClassSubject> query = _context.ClassSubjects;

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
