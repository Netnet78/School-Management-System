using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IStudentQRRepository : IBaseRepository<StudentQR>
    {
        Task<StudentQR?> GetByQRValueAsync(string value);
    }
}
