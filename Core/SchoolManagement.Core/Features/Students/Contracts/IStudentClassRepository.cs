using SchoolManagement.Core.Application.DTOs;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IStudentClassRepository : IBaseRepository<StudentClass>
    {
        Task<IEnumerable<StudentClass>?> GetAllFromStudentIdAsync(int studentId);
        Task<IEnumerable<ClassStudentCountDto>> GetStudentCountPerClass(int currentYear);
    }
}
