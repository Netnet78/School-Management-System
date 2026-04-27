using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IStudentQRRepository : IBaseRepository<StudentQR>
    {
        Task<StudentQR?> GetByQRValueAsync(string value);
    }
}
