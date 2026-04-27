using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class GenerationRepository : IGenerationRepository
    {
        private readonly SchoolDbContext _context;

        public GenerationRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Generation>> GetAllAsync()
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

        public async Task<IEnumerable<Generation>> FindAsync(Expression<Func<Generation, bool>> predicate, int? page, int pageSize, Func<IQueryable<Generation>, IOrderedQueryable<Generation>>? orderBy = null, params Expression<Func<Generation, object>>[] includes)
        {
            IQueryable<Generation> query = _context.Generations;

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
