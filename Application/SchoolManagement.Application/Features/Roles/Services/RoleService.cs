namespace SchoolManagement.Application.Features.Roles.Services
{
    public class RoleService : CrudServiceBase<Role>, IRoleService
    {
        public RoleService(IRoleRepository repository) : base(repository)
        {
        }
    }
}


