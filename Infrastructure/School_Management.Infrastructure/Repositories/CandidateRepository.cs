using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

    public class CandidateRepository : ICandidateRepository
    {
        private readonly SchoolDbContext _context;

        public CandidateRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Candidate>> GetAllAsync(int? lastId, int pageSize)
        {
            IQueryable<Candidate> query = _context.Candidates.OrderBy(c => c.Id).AsQueryable();

            if (lastId.HasValue)
            {
                query = query.Where(c => c.Id > lastId.Value);
            }

            return await query.Take(pageSize).ToListAsync();
        }

        public async Task<List<Candidate>> GetCandidatesOnlyAsync(int? lastId, int pageSize)
        {
            IQueryable<Candidate> query = _context.Candidates.OrderBy(c => c.Id).Where(c => c.Student == null).AsQueryable();

            if (lastId.HasValue)
            {
                query = query.Where(c => c.Id > lastId.Value);
            }
            return await query.Take(pageSize).ToListAsync();
        }

        public async Task<Candidate?> GetByIdAsync(int id)
        {
            return await _context.Candidates.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Candidate candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Candidate candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            Candidate? existing = await _context.Candidates.FindAsync(candidate.Id);
            if (existing == null)
            {
                _context.Candidates.Attach(candidate);
                _context.Entry(candidate).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(candidate);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Candidate candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCandidatesOnlyCountAsync()
        {
            return await _context.Candidates.Where(c => c.Student == null).CountAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await _context.Candidates.CountAsync();
        }
    }
}