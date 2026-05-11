using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

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
