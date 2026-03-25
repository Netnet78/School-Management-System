using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IGenerationRepository
    {
        Task<List<Generation>> GetAllAsync();
        Task<Generation?> GetByIdAsync(int id);
        Task AddAsync(Generation generation);
        Task UpdateAsync(Generation generation);
        Task DeleteAsync(Generation generation);
        Task SaveAsync();
    }

    public class GenerationRepository : IGenerationRepository
    {
        private readonly SchoolDbContext _context;

        public GenerationRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Generation>> GetAllAsync()
        {
            return await _context.Generations.ToListAsync();
        }

        public async Task<Generation?> GetByIdAsync(int id)
        {
            return await _context.Generations.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddAsync(Generation generation)
        {
            ArgumentNullException.ThrowIfNull(generation);
            await _context.Generations.AddAsync(generation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Generation generation)
        {
            ArgumentNullException.ThrowIfNull(generation);
            var existing = await _context.Generations.FindAsync(generation.Id);
            if (existing == null)
            {
                _context.Generations.Attach(generation);
                _context.Entry(generation).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(generation);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Generation generation)
        {
            ArgumentNullException.ThrowIfNull(generation);
            _context.Generations.Remove(generation);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}