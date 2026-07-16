using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Students.Contracts
{
    public interface IStudentClassRepository : IBaseRepository<StudentClass>
    {
        Task<IEnumerable<StudentClass>?> GetAllFromStudentIdAsync(int studentId);
        Task<IEnumerable<ClassStudentCountDto>> GetStudentCountPerClass(int currentYear, int toYear);
    }
}
