using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Employees.ViewModels
{
    public partial class EmployeeUserViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IEmployeeUserService _employeeUserService;
        private readonly IRoleService _roleService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INavigationService _navigationService;
        private readonly IMessageService _messageService;

        private Employee? _employee;

        public Func<string>? GetCreatePassword { get; set; }
        public Func<string>? GetCreateConfirmPassword { get; set; }
        public Func<string>? GetResetPassword { get; set; }
        public Func<string>? GetResetConfirmPassword { get; set; }

        [ObservableProperty]
        private string _employeeName = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasUserAccount;

        [ObservableProperty]
        private bool _canManageUsers;

        [ObservableProperty]
        private User? _userAccount;

        [ObservableProperty]
        private ObservableCollection<Role> _roles = [];

        [ObservableProperty]
        private int _selectedRoleId;

        [ObservableProperty]
        private string _newUsername = string.Empty;

        public string UserStatusText => UserAccount == null ? "No Account" : (UserAccount.IsActive ? "កំពុងដំណើរការ" : "ត្រូវបានបិទ");
        public string UserStatusColor => UserAccount == null ? "Gray" : (UserAccount.IsActive ? "Green" : "Red");
        public string UserRoleName => UserAccount?.Role?.Name ?? "-";
        public string LastLoginText => UserAccount?.LastLogin?.ToString("g") ?? "Never";
        public string IsLockedOut => UserAccount?.LockedOutEnd > DateTime.UtcNow ? "ត្រូវបានផ្អាកការចូលប្រើប្រាស់​មួយរយះ" : "ប្រើប្រាស់បានធម្មតា";

        public EmployeeUserViewModel(
            IEmployeeUserService employeeUserService,
            IRoleService roleService,
            IAuthorizationService authorizationService,
            INavigationService navigationService,
            IMessageService messageService)
        {
            _employeeUserService = employeeUserService;
            _roleService = roleService;
            _authorizationService = authorizationService;
            _navigationService = navigationService;
            _messageService = messageService;
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is EmployeeUserParams p && p.Employee != null)
            {
                _employee = p.Employee;
                EmployeeName = _employee.FullName;
            }
        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            try
            {
                ReturnResponse permission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageUsers);
                if (permission.Status != Status.Success)
                {
                    CanManageUsers = false;
                    _messageService.Show(permission.Message ?? "You do not have permission to manage users.", "Access Denied", MessageButton.OK, MessageIcon.Hand);
                    return;
                }

                CanManageUsers = true;

                await LoadRolesAsync();

                if (_employee != null)
                {
                    ReturnResponse<User?> userResponse = await _employeeUserService.GetUserByEmployeeAsync(_employee.Id);
                    if (userResponse.Status == Status.Success && userResponse.Value != null)
                    {
                        UserAccount = userResponse.Value;
                        HasUserAccount = true;
                        SelectedRoleId = UserAccount.RoleId;
                    }
                    else
                    {
                        HasUserAccount = false;
                        UserAccount = null;
                    }
                }

                RefreshDisplayProperties();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RefreshDisplayProperties()
        {
            OnPropertyChanged(nameof(UserStatusText));
            OnPropertyChanged(nameof(UserStatusColor));
            OnPropertyChanged(nameof(UserRoleName));
            OnPropertyChanged(nameof(LastLoginText));
            OnPropertyChanged(nameof(IsLockedOut));
        }

        private async Task LoadRolesAsync()
        {
            ReturnResponse<IEnumerable<Role>> response = await _roleService.GetAllAsync(
                filters: null,
                page: 1,
                pageSize: int.MaxValue,
                orderBy: [new SortCriteria<Role>("Name")]);

            if (response.Status == Status.Success && response.Value != null)
            {
                Roles.Clear();
                foreach (Role role in response.Value)
                {
                    Roles.Add(role);
                }
            }
        }

        [RelayCommand]
        private async Task CreateUserAsync()
        {
            if (!CanManageUsers || _employee == null) return;

            string password = GetCreatePassword?.Invoke() ?? string.Empty;
            string confirmPassword = GetCreateConfirmPassword?.Invoke() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(NewUsername))
            {
                _messageService.Show("Username is required.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                _messageService.Show("Password must be at least 4 characters.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (password != confirmPassword)
            {
                _messageService.Show("Passwords do not match.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (SelectedRoleId <= 0)
            {
                _messageService.Show("Please select a role.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                ReturnResponse<User> response = await _employeeUserService.CreateUserForEmployeeAsync(
                    _employee.Id, NewUsername.Trim(), password, SelectedRoleId);

                if (response.Status == Status.Success && response.Value != null)
                {
                    UserAccount = response.Value;
                    HasUserAccount = true;
                    NewUsername = string.Empty;
                    RefreshDisplayProperties();

                    _messageService.Show("User account created successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to create user account.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveRoleAsync()
        {
            if (!CanManageUsers || UserAccount == null) return;

            if (SelectedRoleId <= 0)
            {
                _messageService.Show("Please select a role.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                ReturnResponse response = await _employeeUserService.UpdateUserRoleAsync(UserAccount.Id, SelectedRoleId);

                if (response.Status == Status.Success)
                {
                    UserAccount.RoleId = SelectedRoleId;
                    UserAccount.Role = Roles.FirstOrDefault(r => r.Id == SelectedRoleId);
                    OnPropertyChanged(nameof(UserRoleName));
                    _messageService.Show("Role updated successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to update role.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!CanManageUsers || UserAccount == null) return;

            string password = GetResetPassword?.Invoke() ?? string.Empty;
            string confirmPassword = GetResetConfirmPassword?.Invoke() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                _messageService.Show("New password must be at least 4 characters.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (password != confirmPassword)
            {
                _messageService.Show("Passwords do not match.", "Validation Error", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                ReturnResponse response = await _employeeUserService.ResetPasswordAsync(UserAccount.Id, password);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("Password reset successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to reset password.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ToggleActiveAsync()
        {
            if (!CanManageUsers || UserAccount == null) return;

            IsLoading = true;

            try
            {
                ReturnResponse response = await _employeeUserService.ToggleUserActiveStatusAsync(UserAccount.Id);

                if (response.Status == Status.Success)
                {
                    UserAccount.IsActive = !UserAccount.IsActive;
                    OnPropertyChanged(nameof(UserStatusText));
                    OnPropertyChanged(nameof(UserStatusColor));
                    _messageService.Show($"User account {(UserAccount.IsActive ? "activated" : "deactivated")} successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to toggle user status.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task UnlockUserAsync()
        {
            if (!CanManageUsers || UserAccount == null) return;

            IsLoading = true;

            try
            {
                ReturnResponse response = await _employeeUserService.UnlockUserAsync(UserAccount.Id);

                if (response.Status == Status.Success)
                {
                    UserAccount.LockedOutEnd = null;
                    UserAccount.FailedLoginAttempts = 0;
                    OnPropertyChanged(nameof(IsLockedOut));
                    _messageService.Show("User account unlocked successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to unlock user.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteUserAsync()
        {
            if (!CanManageUsers || UserAccount == null) return;

            MessageResult result = _messageService.Show(
                $"Are you sure you want to delete the user account \"{UserAccount.Username}\" for employee \"{EmployeeName}\"?\nThis action cannot be undone.",
                "Confirm Deletion",
                MessageButton.YesNo,
                MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            IsLoading = true;

            try
            {
                ReturnResponse response = await _employeeUserService.DeleteUserAsync(UserAccount.Id);

                if (response.Status == Status.Success)
                {
                    UserAccount = null;
                    HasUserAccount = false;
                    RefreshDisplayProperties();
                    _messageService.Show("User account deleted successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to delete user account.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;
            if (previous != null)
            {
                await _navigationService.NavigateAsync(previous.GetType());
            }
            else
            {
                await _navigationService.NavigateAsync<EmployeeViewModel>();
            }
        }
    }
}
