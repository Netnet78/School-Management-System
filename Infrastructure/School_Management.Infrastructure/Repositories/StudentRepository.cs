using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SchoolDbContext _context;

        public StudentRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            try
            {
                List<Student> students = await _context.Students.ToListAsync();
                return students;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Student student)
        {
            ArgumentNullException.ThrowIfNull(student);

            var existing = await _context.Students.FindAsync(student.Id);
            if (existing == null)
            {
                _context.Students.Attach(student);
                _context.Entry(student).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(student);
            }

            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Student>> FindAsync(Expression<Func<Student, bool>> predicate, int? page, int pageSize, Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderBy = null, params Expression<Func<Student, object>>[] includes)
        {
            IQueryable<Student> query = _context.Students;

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

