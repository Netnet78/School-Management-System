using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IAttendanceRepository
    {
        Task<List<Attendance>> GetAllAsync();
        Task<Attendance?> GetByIdAsync(int id);
        Task AddAsync(Attendance attendance);
        Task UpdateAsync(Attendance attendance);
        Task DeleteAsync(Attendance attendance);
        Task SaveAsync();
    }

    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly SchoolDbContext _context;

        public AttendanceRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Attendance>> GetAllAsync()
        {
            return await _context.Attendances.ToListAsync();
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            return await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Attendance attendance)
        {
            ArgumentNullException.ThrowIfNull(attendance);
            await _context.Attendances.AddAsync(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            ArgumentNullException.ThrowIfNull(attendance);
            var existing = await _context.Attendances.FindAsync(attendance.Id);
            if (existing == null)
            {
                _context.Attendances.Attach(attendance);
                _context.Entry(attendance).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(attendance);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Attendance attendance)
        {
            ArgumentNullException.ThrowIfNull(attendance);
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}