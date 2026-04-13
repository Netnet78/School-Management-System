using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

    public class ScoreRepository : IScoreRepository
    {
        private readonly SchoolDbContext _context;

        public ScoreRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Score>> GetAllAsync()
        {
            return await _context.Scores.ToListAsync();
        }

        public async Task<Score?> GetByIdAsync(int id)
        {
            return await _context.Scores.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Score score)
        {
            ArgumentNullException.ThrowIfNull(score);
            await _context.Scores.AddAsync(score);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Score score)
        {
            ArgumentNullException.ThrowIfNull(score);
            var existing = await _context.Scores.FindAsync(score.Id);
            if (existing == null)
            {
                _context.Scores.Attach(score);
                _context.Entry(score).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(score);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Score score)
        {
            ArgumentNullException.ThrowIfNull(score);
            _context.Scores.Remove(score);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}