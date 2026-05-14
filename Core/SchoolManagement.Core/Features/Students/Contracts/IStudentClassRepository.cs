
using SchoolManagement.Core.Features.Classes.DTOs;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Students.Contracts
{
    public interface IStudentClassRepository : IBaseRepository<StudentClass>
    {
        Task<IEnumerable<StudentClass>?> GetAllFromStudentIdAsync(int studentId);
        Task<IEnumerable<ClassStudentCountDto>> GetStudentCountPerClass(int currentYear);
    }
}

