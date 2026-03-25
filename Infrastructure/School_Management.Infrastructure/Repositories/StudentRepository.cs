using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{

    public interface IStudentRepository
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(Student student);
        Task SaveAsync();
    }

    public class StudentRepository : IStudentRepository
    {
        private readonly SchoolDbContext _context;

        public StudentRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            try
            {
                List<Student> students = await _context.Students.ToListAsync();
                return students;
            }
            catch
            {
                throw;
            }
        }
        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Student student)
        {
            ArgumentNullException.ThrowIfNull(student);

            var existing = await _context.Students.FindAsync(student.Id);
            if (existing == null)
            {
                _context.Students.Attach(student);
                _context.Entry(student).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(student);
            }

            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
