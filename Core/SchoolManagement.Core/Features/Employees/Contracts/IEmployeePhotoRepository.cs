using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IEmployeePhotoRepository : IBaseRepository<EmployeePhoto>
    {
        Task<IEnumerable<EmployeePhoto>> GetPendingDeletes(CancellationToken? token = null);
        Task<IEnumerable<EmployeePhoto>> GetPendingUploads(CancellationToken? token = null);
    }
}
