namespace SchoolManagement.Application.Features.Permissions.Services
{
    public class PermissionService : CrudServiceBase<Permission>, IPermissionService
    {
        public PermissionService(IPermissionRepository repository) : base(repository)
        {

        }
    }
}


