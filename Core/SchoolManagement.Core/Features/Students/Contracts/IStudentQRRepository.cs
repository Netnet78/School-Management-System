using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Students.Contracts
{
    public interface IStudentQRRepository : IBaseRepository<StudentQR>
    {
        Task<StudentQR?> GetByQRValueAsync(string value);
    }
}

