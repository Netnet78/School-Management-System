using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class ExamRepository : IExamRepository
    {
        private readonly SchoolDbContext _context;

        public ExamRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Exam>> GetAllAsync()
        {
            return await _context.Exams.ToListAsync();
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _context.Exams.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Exam exam)
        {
            ArgumentNullException.ThrowIfNull(exam);
            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Exam exam)
        {
            ArgumentNullException.ThrowIfNull(exam);
            var existing = await _context.Exams.FindAsync(exam.Id);
            if (existing == null)
            {
                _context.Exams.Attach(exam);
                _context.Entry(exam).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(exam);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Exam exam)
        {
            ArgumentNullException.ThrowIfNull(exam);
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Exam>> FindAsync(Expression<Func<Exam, bool>> predicate, int? page, int pageSize, Func<IQueryable<Exam>, IOrderedQueryable<Exam>>? orderBy = null, params Expression<Func<Exam, object>>[] includes)
        {
            IQueryable<Exam> query = _context.Exams;

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
