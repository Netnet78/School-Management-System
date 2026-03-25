using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Employee employee);
        Task SaveAsync();
    }

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly SchoolDbContext _context;

        public EmployeeRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Employee employee)
        {
            ArgumentNullException.ThrowIfNull(employee);
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            ArgumentNullException.ThrowIfNull(employee);
            var existing = await _context.Employees.FindAsync(employee.Id);
            if (existing == null)
            {
                _context.Employees.Attach(employee);
                _context.Entry(employee).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(employee);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Employee employee)
        {
            ArgumentNullException.ThrowIfNull(employee);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}