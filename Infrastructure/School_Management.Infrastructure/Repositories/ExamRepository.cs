using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

    public class ExamRepository : IExamRepository
    {
        private readonly SchoolDbContext _context;

        public ExamRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Exam>> GetAllAsync()
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
    }
}