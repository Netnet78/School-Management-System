using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class ScoreRepository : IScoreRepository
    {
        private readonly SchoolDbContext _context;

        public ScoreRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Score>> GetAllAsync()
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

        public async Task<IEnumerable<Score>> FindAsync(Expression<Func<Score, bool>> predicate, int? page, int pageSize, Func<IQueryable<Score>, IOrderedQueryable<Score>>? orderBy = null, params Expression<Func<Score, object>>[] includes)
        {
            IQueryable<Score> query = _context.Scores;

            // Apply includes first
            foreach (var include in includes)
                query = query.Include(include);

            // Apply filter
            query = query.Where(predicate);

            // Apply sorting
            if (orderBy != null)
                query = orderBy(query);

            return await query
                .Skip((pageSize * (page - 1)) ?? pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
