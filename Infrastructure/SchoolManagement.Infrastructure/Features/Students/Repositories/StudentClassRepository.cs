using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Students.Repositories;

public class StudentClassRepository : BaseRepository<StudentClass>, IStudentClassRepository
{
    public StudentClassRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StudentClass>?> GetAllFromStudentIdAsync(int studentId)
    {
        Student? student = await Context.Students
            .Include(s => s.Classes)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
        {
            return null;
        }

        return student.Classes.ToList();
    }

    public async Task<IEnumerable<ClassStudentCountDto>> GetStudentCountPerClass(int fromYear, int toYear)
    {
        return await Context.Classes
            .Include(c => c.Generation)
            .ThenInclude(g => g.Department)
            .Include(c => c.Grade)
            .Where(c => c.Generation.AcademicStartYear >= fromYear &&
                c.Generation.AcademicStartYear <= toYear && c.Students.Count() > 0)
            .Select(c => new ClassStudentCountDto
            {
                ClassName = $"{c.Grade.KhmerName} {c.Generation.Department.KhmerName} ជំនាន់ទី " +
                $"{c.Generation.CohortNumber}",
                Count = c.Students.Count()
            })
            .ToListAsync();
    }
}
