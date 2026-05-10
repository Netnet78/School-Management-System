using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IEmployeePhotoRepository : IBaseRepository<EmployeePhoto>
    {
        Task<IEnumerable<EmployeePhoto>> GetPendingDeletes(CancellationToken? token = null);
        Task<IEnumerable<EmployeePhoto>> GetPendingUploads(CancellationToken? token = null);
    }
}
