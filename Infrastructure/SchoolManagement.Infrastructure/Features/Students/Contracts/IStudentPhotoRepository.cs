using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Students.Contracts
{
    public interface IStudentPhotoRepository : IBaseRepository<StudentPhoto>
    {
        Task<IEnumerable<StudentPhoto>> GetPendingUploads(CancellationToken? token = null);
        Task<IEnumerable<StudentPhoto>> GetPendingDeletes(CancellationToken? token = null);
    }
}
