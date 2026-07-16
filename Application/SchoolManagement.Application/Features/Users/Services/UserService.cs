namespace SchoolManagement.Application.Features.Users.Services
{
    public class UserService : CrudServiceBase<User>, IUserService
    {
        public UserService(IUserRepository repository) : base(repository)
        {
        }
    }
}


