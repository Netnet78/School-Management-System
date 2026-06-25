using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Roles.ViewModels
{
    public partial class RoleManagementViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;

        [ObservableProperty]
        private ObservableCollection<Role> _roles = [];

        [ObservableProperty]
        private ObservableCollection<PermissionItem> _permissionItems = [];

        [ObservableProperty]
        private Role? _selectedRole;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canManageRoles;

        [ObservableProperty]
        private string _newRoleName = string.Empty;

        [ObservableProperty]
        private string _newRoleDescription = string.Empty;

        [ObservableProperty]
        private bool _isAddingNewRole;

        [ObservableProperty]
        private bool _isEditingRole;

        [ObservableProperty]
        private string _editingRoleName = string.Empty;

        [ObservableProperty]
        private string _editingRoleDescription = string.Empty;

        public RoleManagementViewModel(
            IRoleService roleService,
            IPermissionService permissionService,
            IAuthorizationService authorizationService,
            IMessageService messageService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _authorizationService = authorizationService;
            _messageService = messageService;
        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            try
            {
                ReturnResponse permission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageRoles);
                if (permission.Status != Status.Success)
                {
                    CanManageRoles = false;
                    _messageService.Show(permission.Message ?? "You do not have permission to manage roles.", "Access Denied", MessageButton.OK, MessageIcon.Hand);
                    return;
                }

                CanManageRoles = true;

                await LoadPermissionsAsync();
                await LoadRolesAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRolesAsync()
        {
            ReturnResponse<IEnumerable<Role>> response = await _roleService.GetAllAsync(
                filters: null,
                page: 1,
                pageSize: int.MaxValue,
                orderBy: [new SortCriteria<Role>("Name")],
                includes: ["Permissions"]);

            if (response.Status == Status.Success && response.Value != null)
            {
                Roles.Clear();
                foreach (Role role in response.Value)
                {
                    Roles.Add(role);
                }

                RefreshPermissionItems();
            }
        }

        private async Task LoadPermissionsAsync()
        {
            ReturnResponse<IEnumerable<Permission>> response = await _permissionService.GetAllAsync(
                filters: null,
                page: 1,
                pageSize: int.MaxValue,
                orderBy: [new SortCriteria<Permission>("Name")]);

            if (response.Status == Status.Success && response.Value != null)
            {
                AllPermissions.Clear();
                foreach (Permission permission in response.Value)
                {
                    AllPermissions.Add(permission);
                }
            }
        }

        private List<Permission> AllPermissions { get; } = [];

        private void RefreshPermissionItems()
        {
            PermissionItems.Clear();

            if (SelectedRole == null) return;

            HashSet<int> assignedIds = SelectedRole.Permissions?
                .Select(p => p.Id)
                .ToHashSet() ?? [];

            foreach (Permission permission in AllPermissions)
            {
                PermissionItems.Add(new PermissionItem(permission, assignedIds.Contains(permission.Id)));
            }
        }

        partial void OnSelectedRoleChanged(Role? value)
        {
            if (value != null)
            {
                EditingRoleName = value.Name;
                EditingRoleDescription = value.Description ?? string.Empty;
                IsEditingRole = true;
                RefreshPermissionItems();
            }
            else
            {
                IsEditingRole = false;
                PermissionItems.Clear();
            }
        }

        [RelayCommand]
        private async Task TogglePermissionAsync(PermissionItem? item)
        {
            if (!CanManageRoles || SelectedRole == null || item == null) return;

            IsLoading = true;

            try
            {
                List<int> currentIds = SelectedRole.Permissions?.Select(p => p.Id).ToList() ?? [];
                List<int> updatedIds;

                if (currentIds.Contains(item.Permission.Id))
                {
                    updatedIds = currentIds.Where(id => id != item.Permission.Id).ToList();
                }
                else
                {
                    updatedIds = [.. currentIds, item.Permission.Id];
                }

                ReturnResponse response = await _roleService.UpdateRolePermissionsAsync(SelectedRole.Id, updatedIds);

                if (response.Status == Status.Success)
                {
                    int selectedRoleId = SelectedRole.Id;
                    await LoadRolesAsync();
                    SelectedRole = Roles.FirstOrDefault(r => r.Id == selectedRoleId);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to update permissions.", "Error", MessageButton.OK, MessageIcon.Error);
                    RefreshPermissionItems();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddRoleAsync()
        {
            if (!CanManageRoles) return;

            if (string.IsNullOrWhiteSpace(NewRoleName))
            {
                _messageService.Show("Role name is required.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                Role role = new()
                {
                    Name = NewRoleName.Trim(),
                    Description = NewRoleDescription?.Trim(),
                    Permissions = []
                };

                ReturnResponse response = await _roleService.InsertAsync(role);

                if (response.Status == Status.Success)
                {
                    await LoadRolesAsync();
                    NewRoleName = string.Empty;
                    NewRoleDescription = string.Empty;
                    IsAddingNewRole = false;
                    _messageService.Show("Role added successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to add role.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveEditRoleAsync()
        {
            if (!CanManageRoles || SelectedRole == null) return;

            if (string.IsNullOrWhiteSpace(EditingRoleName))
            {
                _messageService.Show("Role name is required.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                string originalName = SelectedRole.Name;
                string? originalDescription = SelectedRole.Description;

                SelectedRole.Name = EditingRoleName.Trim();
                SelectedRole.Description = EditingRoleDescription?.Trim();

                ReturnResponse response = await _roleService.UpdateAsync(SelectedRole);

                if (response.Status == Status.Success)
                {
                    int index = Roles.IndexOf(SelectedRole);
                    if (index >= 0)
                    {
                        Roles.RemoveAt(index);
                        Roles.Insert(index, SelectedRole);
                    }
                    _messageService.Show("Role updated successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    SelectedRole.Name = originalName;
                    SelectedRole.Description = originalDescription;
                    _messageService.Show(response.Message ?? "Unable to update role.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteRoleAsync(Role? role)
        {
            if (!CanManageRoles || role == null) return;

            MessageResult result = _messageService.Show(
                $"Are you sure you want to delete the role \"{role.Name}\"?\nThis action cannot be undone.",
                "Confirm Deletion",
                MessageButton.YesNo,
                MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            IsLoading = true;

            try
            {
                ReturnResponse response = await _roleService.DeleteAsync(role);

                if (response.Status == Status.Success)
                {
                    Roles.Remove(role);
                    if (SelectedRole?.Id == role.Id)
                    {
                        SelectedRole = null;
                    }
                    _messageService.Show("Role deleted successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to delete role.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleAddNewRole()
        {
            IsAddingNewRole = !IsAddingNewRole;
            if (!IsAddingNewRole)
            {
                NewRoleName = string.Empty;
                NewRoleDescription = string.Empty;
            }
        }
    }
}
