using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Accessments.Contracts
{
    public interface IAccessmentRepository : IBaseRepository<Assessment>
    {
    }
}
