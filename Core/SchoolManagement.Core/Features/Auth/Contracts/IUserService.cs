
using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Auth.Contracts
{
    public interface IUserService : ICrudService<User>
    {
    }
}

