using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Employees.Contracts
{
    public interface IEmployeePhotoRepository : IBaseRepository<EmployeePhoto>
    {
        Task<IEnumerable<EmployeePhoto>> GetPendingDeletes(CancellationToken? token = null);
        Task<IEnumerable<EmployeePhoto>> GetPendingUploads(CancellationToken? token = null);
    }
}
