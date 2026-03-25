using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(Department department);
        Task SaveAsync();
    }

    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly SchoolDbContext _context;

        public DepartmentRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);
            var existing = await _context.Departments.FindAsync(department.Id);
            if (existing == null)
            {
                _context.Departments.Attach(department);
                _context.Entry(department).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(department);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}