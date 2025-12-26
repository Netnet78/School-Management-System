using Microsoft.EntityFrameworkCore;
using Student_Management.Data;
using Student_Management.Models;

namespace Student_Management.Services
{

    public interface IStudentRepository
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task AddStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(Student student);
        Task SaveStudentAsync();
    }

    public class StudentRepository : IStudentRepository
    {
        private readonly StudentDbContext _context;

        public StudentRepository(StudentDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> GetAllStudentsAsync() => await _context.Students.ToListAsync();
        public async Task AddStudentAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateStudentAsync(Student student)
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
        public async Task DeleteStudentAsync(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
        public async Task SaveStudentAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
