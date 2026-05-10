using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories;

public class StudentQRRepository : BaseRepository<StudentQR>, IStudentQRRepository
{
    public StudentQRRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<StudentQR?> GetByQRValueAsync(string value)
    {
        return await Set
            .Include(s => s.Student)
            .Include(s => s.Student.Candidate)
            .Include(s => s.Student.Candidate.Skill)
            .FirstOrDefaultAsync(s => s.QRCodeValue == value);
    }
}
