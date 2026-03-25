using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface ICandidateRepository
    {
        Task<List<Candidate>> GetAllAsync();
        Task<List<Candidate>> GetCandidatesOnlyAsync();
        Task<Candidate?> GetByIdAsync(int id);
        Task AddAsync(Candidate candidate);
        Task UpdateAsync(Candidate candidate);
        Task DeleteAsync(Candidate candidate);
        Task SaveAsync();
    }

    public class CandidateRepository : ICandidateRepository
    {
        private readonly SchoolDbContext _context;

        public CandidateRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Candidate>> GetAllAsync()
        {
            return await _context.Candidates.ToListAsync();
        }

        public async Task<List<Candidate>> GetCandidatesOnlyAsync()
        {
            return await _context.Candidates.Where(c => c.Student == null).ToListAsync();
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
            var existing = await _context.Candidates.FindAsync(candidate.Id);
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
    }
}