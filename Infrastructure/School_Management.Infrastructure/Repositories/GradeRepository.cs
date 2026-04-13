using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

    public class GradeRepository : IGradeRepository
    {
        private readonly SchoolDbContext _context;

        public GradeRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Grade>> GetAllAsync()
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
    }
}