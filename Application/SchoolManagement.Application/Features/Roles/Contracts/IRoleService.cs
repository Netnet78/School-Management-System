namespace SchoolManagement.Application.Features.Roles.Contracts
{
    public interface IRoleService : ICrudService<Role>
    {
        Task<ReturnResponse> UpdateRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds);
    }
}
