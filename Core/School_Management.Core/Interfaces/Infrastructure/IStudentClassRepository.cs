using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IStudentClassRepository : IBaseRepository<StudentClass>
    {
        Task<IEnumerable<StudentClass>?> GetAllFromStudentIdAsync(int studentId);
    }
}
