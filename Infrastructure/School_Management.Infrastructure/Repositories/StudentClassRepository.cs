using Microsoft.EntityFrameworkCore;
using School_Management.Core.DTOs;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories;

public class StudentClassRepository : BaseRepository<StudentClass>, IStudentClassRepository
{
    public StudentClassRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StudentClass>?> GetAllFromStudentIdAsync(int studentId)
    {
        Student? student = await Context.Students.FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
        {
            return null;
        }

        return student.Classes.ToList();
    }

    public async Task<IEnumerable<ClassStudentCountDto>> GetStudentCountPerClass(int currentYear)
    {
        return await Context.Classes
            .Where(c => currentYear >= c.Generation.AcademicStartYear &&
                currentYear <= c.Generation.AcademicEndYear)
            .Select(c => new ClassStudentCountDto
            {
                ClassName = c.Name,
                Count = c.Students.Count
            })
            .ToListAsync();
    }
}
