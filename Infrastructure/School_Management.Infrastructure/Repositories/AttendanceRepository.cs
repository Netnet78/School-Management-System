using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories;

public class AttendanceRepository : BaseRepository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Attendance>> GetAllFromStudentId(int studentId)
    {
        Student? student = await Context.Students
            .Include(s => s.Classes)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        List<Attendance>? attendances = student?.Classes.SelectMany(s => s.Attendances).ToList();
        return attendances ?? [];
    }

    public async Task<IEnumerable<Attendance>> GetAllFromStudentClassId(int studentClassId)
    {
        StudentClass? studentClass = await Context.StudentClasses.FirstOrDefaultAsync(sc => sc.Id == studentClassId);
        List<Attendance>? attendances = studentClass?.Attendances.ToList();
        return attendances ?? [];
    }

    public async Task<Attendance?> GetByStudentClassId(int studentClassId)
    {
        return await Context.Attendances.FirstOrDefaultAsync(a => a.StudentClassId == studentClassId);
    }
}
