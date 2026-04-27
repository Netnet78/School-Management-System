using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class StudentClassRepository : IStudentClassRepository
    {
        private readonly SchoolDbContext _context;

        public StudentClassRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentClass>> GetAllAsync()
        {
            return await _context.StudentClasses.ToListAsync();
        }

        public async Task<StudentClass?> GetByIdAsync(int id)
        {
            return await _context.StudentClasses.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(StudentClass studentClass)
        {
            ArgumentNullException.ThrowIfNull(studentClass);
            await _context.StudentClasses.AddAsync(studentClass);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StudentClass studentClass)
        {
            ArgumentNullException.ThrowIfNull(studentClass);
            var existing = await _context.StudentClasses.FindAsync(studentClass.Id);
            if (existing == null)
            {
                _context.StudentClasses.Attach(studentClass);
                _context.Entry(studentClass).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(studentClass);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(StudentClass studentClass)
        {
            ArgumentNullException.ThrowIfNull(studentClass);
            _context.StudentClasses.Remove(studentClass);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StudentClass>?> GetAllFromStudentIdAsync(int studentId)
        {
            Student? student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            List<StudentClass> studentClasses = student.Classes.ToList();

            return studentClasses;
        }

        public async Task<IEnumerable<StudentClass>> FindAsync(Expression<Func<StudentClass, bool>> predicate, int? page, int pageSize, Func<IQueryable<StudentClass>, IOrderedQueryable<StudentClass>>? orderBy = null, params Expression<Func<StudentClass, object>>[] includes)
        {
            IQueryable<StudentClass> query = _context.StudentClasses;

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
