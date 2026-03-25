using Microsoft.EntityFrameworkCore;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Infrastructure.Repositories
{
    public interface ISkillRepository
    {
        Task AddAsync(Skill skill);
        Task DeleteAsync(Skill skill);
        Task<List<Skill>> GetAllAsync();
        Task<Skill?> GetByIdAsync(int id);
        Task SaveAsync();
        Task UpdateAsync(Skill skill);
    }

    public class SkillRepository : ISkillRepository
    {
        private readonly SchoolDbContext _context;

        public SkillRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Skill>> GetAllAsync()
        {
            return await _context.Skills.ToListAsync();
        }

        public async Task<Skill?> GetByIdAsync(int id)
        {
            return await _context.Skills.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Skill skill)
        {
            ArgumentNullException.ThrowIfNull(skill);
            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Skill skill)
        {
            ArgumentNullException.ThrowIfNull(skill);
            var existing = await _context.Skills.FindAsync(skill.Id);
            if (existing == null)
            {
                _context.Skills.Attach(skill);
                _context.Entry(skill).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(skill);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Skill skill)
        {
            ArgumentNullException.ThrowIfNull(skill);
            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
