using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Students.Repositories;

public class StudentQRRepository : BaseRepository<StudentQR>, IStudentQRRepository
{
    public StudentQRRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<StudentQR?> GetByQRValueAsync(string value)
    {
        return await Set
            .Include(s => s.Student)
            .ThenInclude(student => student.Candidate)
            .ThenInclude(candidate => candidate.Photo)
            .Include(s => s.Student)
            .ThenInclude(student => student.Candidate)
            .ThenInclude(candidate => candidate.Skill)
            .FirstOrDefaultAsync(s => s.QRCodeValue == value);
    }
}
