using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class StudentRepository : BaseRepository<Student>, IStudentRepository
{
    public StudentRepository(SchoolDbContext context) : base(context)
    {
    }
}
