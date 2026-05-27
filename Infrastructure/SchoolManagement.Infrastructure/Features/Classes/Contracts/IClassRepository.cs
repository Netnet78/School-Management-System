using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Classes.Contracts
{
    public interface IClassRepository : IBaseRepository<Class>
    {
        Task<Class?> GetByIdWithSubjectsAsync(int id);
    }
}
