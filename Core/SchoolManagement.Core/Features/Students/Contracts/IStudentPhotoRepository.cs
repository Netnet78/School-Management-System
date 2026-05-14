using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Students.Contracts
{
    public interface IStudentPhotoRepository : IBaseRepository<StudentPhoto>
    {
        Task<IEnumerable<StudentPhoto>> GetPendingUploads(CancellationToken? token = null);
        Task<IEnumerable<StudentPhoto>> GetPendingDeletes(CancellationToken? token = null);
    }
}

