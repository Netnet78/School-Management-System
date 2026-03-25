using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IStudentClassRepository
    {
        Task<List<StudentClass>> GetAllAsync();
        Task<StudentClass?> GetByIdAsync(int id);
        Task AddAsync(StudentClass studentClass);
        Task UpdateAsync(StudentClass studentClass);
        Task DeleteAsync(StudentClass studentClass);
        Task SaveAsync();
    }

    public class StudentClassRepository : IStudentClassRepository
    {
        private readonly SchoolDbContext _context;

        public StudentClassRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentClass>> GetAllAsync()
        {
            return await _context.StudentClasses.ToListAsync();
        }

        public async Task<StudentClass?> GetByIdAsync(int id)
        {
            return await _context.StudentClasses.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(StudentClass studentClass)
        {
            ArgumentNullException.ThrowIfNull(studentClass);
            await _context.StudentClasses.AddAsync(studentClass);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StudentClass studentClass)
        {
            ArgumentNullException.ThrowIfNull(studentClass);
            var existing = await _context.StudentClasses.FindAsync(studentClass.Id);
            if (existing == null)
            {
                _context.StudentClasses.Attach(studentClass);
                _context.Entry(studentClass).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(studentClass);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(StudentClass studentClass)
        {
            ArgumentNullException.ThrowIfNull(studentClass);
            _context.StudentClasses.Remove(studentClass);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}