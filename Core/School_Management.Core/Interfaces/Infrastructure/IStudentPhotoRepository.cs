using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IStudentPhotoRepository : IBaseRepository<StudentPhoto>
    {
        Task<IEnumerable<StudentPhoto>> GetPendingUploads(CancellationToken? token = null);
        Task<IEnumerable<StudentPhoto>> GetPendingDeletes(CancellationToken? token = null);
    }
}
