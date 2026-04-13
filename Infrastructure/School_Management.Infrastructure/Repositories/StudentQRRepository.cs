using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

    public class StudentQRRepository : IStudentQRRepository
    {
        private readonly SchoolDbContext _context;

        public StudentQRRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentQR>> GetAllAsync()
        {
            return await _context.StudentQRs.ToListAsync();
        }

        public async Task<StudentQR?> GetByIdAsync(int id)
        {
            return await _context.StudentQRs.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<StudentQR?> GetByQRValueAsync(string value)
        {
            return await _context.StudentQRs
                .Include(s => s.Student)
                .Include(s => s.Student.Candidate)
                .Include(s => s.Student.Candidate.Skill)
                .FirstOrDefaultAsync(s => s.QRCodeValue == value);
        }

        public async Task AddAsync(StudentQR studentQR)
        {
            ArgumentNullException.ThrowIfNull(studentQR);
            await _context.StudentQRs.AddAsync(studentQR);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StudentQR studentQR)
        {
            ArgumentNullException.ThrowIfNull(studentQR);
            var existing = await _context.StudentQRs.FindAsync(studentQR.Id);
            if (existing == null)
            {
                _context.StudentQRs.Attach(studentQR);
                _context.Entry(studentQR).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(studentQR);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(StudentQR studentQR)
        {
            ArgumentNullException.ThrowIfNull(studentQR);
            _context.StudentQRs.Remove(studentQR);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}