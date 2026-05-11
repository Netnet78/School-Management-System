using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IStudentPhotoRepository : IBaseRepository<StudentPhoto>
    {
        Task<IEnumerable<StudentPhoto>> GetPendingUploads(CancellationToken? token = null);
        Task<IEnumerable<StudentPhoto>> GetPendingDeletes(CancellationToken? token = null);
    }
}
