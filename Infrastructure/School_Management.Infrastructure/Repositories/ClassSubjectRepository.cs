using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IClassSubjectRepository
    {
        Task<List<ClassSubject>> GetAllAsync();
        Task<ClassSubject?> GetByIdAsync(int id);
        Task AddAsync(ClassSubject classSubject);
        Task UpdateAsync(ClassSubject classSubject);
        Task DeleteAsync(ClassSubject classSubject);
        Task SaveAsync();
    }

    public class ClassSubjectRepository : IClassSubjectRepository
    {
        private readonly SchoolDbContext _context;

        public ClassSubjectRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClassSubject>> GetAllAsync()
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
    }
}