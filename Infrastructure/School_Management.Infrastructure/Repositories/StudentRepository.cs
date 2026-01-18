using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{

    public interface IStudentRepository
    {
        Task<List<Candidate>> GetAllStudentsAsync();
        Task AddStudentAsync(Candidate student);
        Task UpdateStudentAsync(Candidate student);
        Task DeleteStudentAsync(Candidate student);
        Task SaveStudentAsync();
    }

    public class StudentRepository : IStudentRepository
    {
        private readonly StudentDbContext _context;

        public StudentRepository(StudentDbContext context)
        {
            _context = context;
        }

        public async Task<List<Candidate>> GetAllStudentsAsync()
        {
            try
            {
                List<Candidate> students = await _context.Candidates.ToListAsync();
                return students;
            }
            catch
            {
                throw;
            }
        }
        public async Task AddStudentAsync(Candidate student)
        {
            await _context.Candidates.AddAsync(student);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateStudentAsync(Candidate student)
        {
            ArgumentNullException.ThrowIfNull(student);

            var existing = await _context.Candidates.FindAsync(student.Id);
            if (existing == null)
            {
                _context.Candidates.Attach(student);
                _context.Entry(student).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(student);
            }

            await _context.SaveChangesAsync();
        }
        public async Task DeleteStudentAsync(Candidate student)
        {
            _context.Candidates.Remove(student);
            await _context.SaveChangesAsync();
        }
        public async Task SaveStudentAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
