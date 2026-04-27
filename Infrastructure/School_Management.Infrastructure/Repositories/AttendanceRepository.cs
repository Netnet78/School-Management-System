using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly SchoolDbContext _context;

        public AttendanceRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _context.Attendances.ToListAsync();
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            return await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Attendance>> GetAllFromStudentId(int studentId)
        {
            Student? student = await _context.Students.Include(s => s.Classes).FirstOrDefaultAsync(s => s.Id == studentId);
            List<Attendance>? attendances = student?.Classes.SelectMany(s => s.Attendances).ToList();
            return attendances ?? [];
        }

        public async Task<IEnumerable<Attendance>> GetAllFromStudentClassId(int studentClassId)
        {
            StudentClass? studentClass = await _context.StudentClasses.FirstOrDefaultAsync(sc => sc.Id == studentClassId);
            List<Attendance>? attendances = studentClass?.Attendances.ToList();
            return attendances ?? [];
        }

        public async Task<Attendance?> GetByStudentClassId(int studentClassId)
        {
            return await _context.Attendances.FirstOrDefaultAsync(a => a.StudentClassId == studentClassId);
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

        public async Task<IEnumerable<Attendance>> FindAsync(Expression<Func<Attendance, bool>> predicate, int? page, int pageSize, Func<IQueryable<Attendance>, IOrderedQueryable<Attendance>>? orderBy = null, params Expression<Func<Attendance, object>>[] includes)
        {
            IQueryable<Attendance> query = _context.Attendances;

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
