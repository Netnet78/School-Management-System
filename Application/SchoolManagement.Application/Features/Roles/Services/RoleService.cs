namespace SchoolManagement.Application.Features.Roles.Services
{
    public class RoleService : CrudServiceBase<Role>, IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository repository) : base(repository)
        {
            _roleRepository = repository;
        }

        public async Task<ReturnResponse> UpdateRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
        {
            try
            {
                await _roleRepository.UpdateRolePermissionsAsync(roleId, permissionIds);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }
    }
}


