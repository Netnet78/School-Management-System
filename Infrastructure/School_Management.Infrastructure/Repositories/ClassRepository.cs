using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IClassRepository
    {
        Task<List<Class>> GetAllAsync();
        Task<Class?> GetByIdAsync(int id);
        Task AddAsync(Class @class);
        Task UpdateAsync(Class @class);
        Task DeleteAsync(Class @class);
        Task SaveAsync();
    }

    public class ClassRepository : IClassRepository
    {
        private readonly SchoolDbContext _context;

        public ClassRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Class>> GetAllAsync()
        {
            return await _context.Classes.ToListAsync();
        }

        public async Task<Class?> GetByIdAsync(int id)
        {
            return await _context.Classes.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Class @class)
        {
            ArgumentNullException.ThrowIfNull(@class);
            await _context.Classes.AddAsync(@class);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Class @class)
        {
            ArgumentNullException.ThrowIfNull(@class);
            var existing = await _context.Classes.FindAsync(@class.Id);
            if (existing == null)
            {
                _context.Classes.Attach(@class);
                _context.Entry(@class).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(@class);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Class @class)
        {
            ArgumentNullException.ThrowIfNull(@class);
            _context.Classes.Remove(@class);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}