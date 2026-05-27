using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Students.Contracts
{
    public interface IStudentQRRepository : IBaseRepository<StudentQR>
    {
        Task<StudentQR?> GetByQRValueAsync(string value);
    }
}
