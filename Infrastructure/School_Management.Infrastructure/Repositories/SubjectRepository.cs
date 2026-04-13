using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

    public class SubjectRepository : ISubjectRepository
    {
        private readonly SchoolDbContext _context;

        public SubjectRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subject>> GetAllAsync()
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
    }
}