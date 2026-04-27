using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class SubjectRepository : ISubjectRepository
    {
        private readonly SchoolDbContext _context;

        public SubjectRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subject>> GetAllAsync()
        {
            return await _context.Subjects.ToListAsync();
        }

        public async Task<Subject?> GetByIdAsync(int id)
        {
            return await _context.Subjects.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Subject subject)
        {
            ArgumentNullException.ThrowIfNull(subject);
            await _context.Subjects.AddAsync(subject);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Subject subject)
        {
            ArgumentNullException.ThrowIfNull(subject);
            var existing = await _context.Subjects.FindAsync(subject.Id);
            if (existing == null)
            {
                _context.Subjects.Attach(subject);
                _context.Entry(subject).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(subject);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Subject subject)
        {
            ArgumentNullException.ThrowIfNull(subject);
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Subject>> FindAsync(Expression<Func<Subject, bool>> predicate, int? page, int pageSize, Func<IQueryable<Subject>, IOrderedQueryable<Subject>>? orderBy = null, params Expression<Func<Subject, object>>[] includes)
        {
            IQueryable<Subject> query = _context.Subjects;

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
