using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
    }
}
