using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Departments.Models;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Employees.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private const int DefaultPageSize = 10;

        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IEmployeeUserService _employeeUserService;

        private ObservableCollection<Employee> _allEmployees = [];
        private CancellationTokenSource? _cts;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = [];

        [ObservableProperty]
        private ObservableCollection<Department> _departments = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canManageEmployees;

        [ObservableProperty]
        private bool _canManageUsers;

        [ObservableProperty]
        private bool _canFilterDepartment;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int? _selectedDepartmentFilterId;

        [ObservableProperty]
        private bool? _selectedActiveFilter;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _maxPage = 1;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _currentPageTotalCount;

        [ObservableProperty]
        private string _pageCount = string.Empty;

        public IEnumerable<object> ActiveFilterOptions { get; } =
        [
            new { Value = (bool?)null, Description = "All" },
            new { Value = (bool?)true, Description = "Active" },
            new { Value = (bool?)false, Description = "Inactive" }
        ];

        public EmployeeViewModel(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService,
            IDispatcherService dispatcherService,
            IEmployeeUserService employeeUserService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;
            _employeeUserService = employeeUserService;
        }

        public async Task LoadAsync()
        {
            ReturnResponse permission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageEmployees);
            if (permission.Status != Status.Success)
            {
                CanManageEmployees = false;
                _messageService.Show(permission.Message ?? "You do not have permission to manage employees.", "Access denied", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            CanManageEmployees = true;

            // Check ManageUsers permission separately
            ReturnResponse userPermission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageUsers);
            CanManageUsers = userPermission.Status == Status.Success;

            User? currentUser = _authorizationService.CurrentUser;
            if (currentUser == null)
            {
                _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            CanFilterDepartment = currentUser.IsAdmin();

            await LoadDepartmentsAsync();
            await LoadEmployeesAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadEmployeesAsync();
        }

        [RelayCommand]
        private async Task AddEmployeeAsync()
        {
            if (!CanManageEmployees)
            {
                return;
            }

            await _navigationService.NavigateAsync<AddEmployeeViewModel>();
        }

        [RelayCommand]
        private async Task EditEmployeeAsync(Employee? employee)
        {
            if (!CanManageEmployees || employee == null)
            {
                return;
            }

            await _navigationService.NavigateAsync<EditEmployeeViewModel>(new EditEmployeeParams { Employee = employee });
        }

        [RelayCommand]
        private async Task ManageUserAsync(Employee? employee)
        {
            if (!CanManageUsers || employee == null)
            {
                return;
            }

            await _navigationService.NavigateAsync<EmployeeUserViewModel>(new EmployeeUserParams { Employee = employee });
        }

        [RelayCommand]
        private async Task DeleteEmployeeAsync(Employee? employee)
        {
            if (!CanManageEmployees || employee == null)
            {
                return;
            }

            MessageResult result = _messageService.Show(
                $"What do you want to do with employee \"{employee.FullName}\"?\n\nYes = delete from database\nNo = set inactive\nCancel = keep as-is",
                "Employee action",
                MessageButton.YesNoCancel,
                MessageIcon.Question);

            if (result == MessageResult.Cancel)
            {
                return;
            }

            IsLoading = true;

            try
            {
                ReturnResponse response;

                if (result == MessageResult.Yes)
                {
                    response = await _employeeService.DeleteAsync(employee);

                    if (response.Status == Status.Success)
                    {
                        _allEmployees.Remove(employee);
                        await ApplyFiltersAndPaging();
                        _messageService.Show("Employee deleted successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                    }
                    else
                    {
                        _messageService.Show(response.Message ?? "Unable to delete the employee.", "Error", MessageButton.OK, MessageIcon.Error);
                    }
                }
                else
                {
                    employee.IsActive = false;
                    response = await _employeeService.UpdateAsync(employee);

                    if (response.Status == Status.Success)
                    {
                        _messageService.Show("Employee marked as inactive successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                        await ApplyFiltersAndPaging();
                    }
                    else
                    {
                        _messageService.Show(response.Message ?? "Unable to mark the employee as inactive.", "Error", MessageButton.OK, MessageIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while deleting the employee: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ResetFiltersAsync()
        {
            SearchText = string.Empty;
            SelectedActiveFilter = null;
            SelectedDepartmentFilterId = CanFilterDepartment ? null : _authorizationService.CurrentUser?.Employee?.DepartmentId;
            CurrentPage = 1;
            await ApplyFiltersAndPaging();
        }

        async partial void OnSearchTextChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                await Task.Delay(300, token);
                await RefreshOnFilterAsync();
            }
            catch (TaskCanceledException) { }
        }

        partial void OnSelectedDepartmentFilterIdChanged(int? value)
        {
            CurrentPage = 1;
            Task.Run(() => ApplyFiltersAndPaging());
        }

        partial void OnSelectedActiveFilterChanged(bool? value)
        {
            CurrentPage = 1;
            Task.Run(() => ApplyFiltersAndPaging());
        }

        partial void OnCurrentPageChanged(int oldValue, int newValue)
        {
            if (IsLoading)
            {
                return;
            }

            if (newValue < 1)
            {
                CurrentPage = 1;
                return;
            }

            if (newValue > MaxPage)
            {
                CurrentPage = MaxPage;
                return;
            }

            Task.Run(() => ApplyFiltersAndPaging());
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                User? currentUser = _authorizationService.CurrentUser;
                if (currentUser == null)
                {
                    _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }
                ReturnResponse<IEnumerable<Department>> response = CanFilterDepartment
                    ? await _departmentService.GetAllAsync(
                        filters: null,
                        page: 1,
                        pageSize: int.MaxValue,
                        orderBy: [new SortCriteria<Department>("KhmerName")])
                    : await _departmentService.GetAllAsync(
                        filters: [new FilterCondition<Department>(d => d.Id, FilterOperator.Equals, currentUser.Employee!.DepartmentId)],
                        page: 1,
                        pageSize: int.MaxValue,
                        orderBy: [new SortCriteria<Department>("KhmerName")]);

            if (response.Status == Status.Success && response.Value != null)
            {
                Departments.Clear();
                foreach (Department department in response.Value)
                {
                        Departments.Add(department);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading departments: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
        }

        private async Task LoadEmployeesAsync()
        {
            IsLoading = true;

            try
            {
                User? currentUser = _authorizationService.CurrentUser;
                if (currentUser == null)
                {
                    _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                List<FilterCondition<Employee>>? accessFilters = null;
                if (currentUser?.IsHeadTeacher() == true)
                {
                    int? departmentId = currentUser.Employee?.DepartmentId;
                    if (!departmentId.HasValue)
                    {
                        _messageService.Show("Unable to determine the current department.", "Error", MessageButton.OK, MessageIcon.Error);
                        return;
                    }

                    accessFilters = [new FilterCondition<Employee>(e => e.DepartmentId, FilterOperator.Equals, departmentId.Value)];
                    if (!CanFilterDepartment)
                    {
                        SelectedDepartmentFilterId = departmentId;
                    }
                }

                ReturnResponse<IEnumerable<Employee>> response = await _employeeService.GetAllAsync(
                    filters: accessFilters,
                    page: 1,
                    pageSize: int.MaxValue,
                    orderBy: [new SortCriteria<Employee>("FullName")],
                    includes: ["Department", "Photo"]);

                if (response.Status != Status.Success || response.Value == null)
                {
                    _messageService.Show(response.Message ?? "Unable to load employees.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                _allEmployees = new ObservableCollection<Employee>(response.Value);
                CurrentPage = 1;
                await ApplyFiltersAndPaging();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading employees: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApplyFiltersAndPaging()
        {
            IEnumerable<Employee> query = _allEmployees;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                query = query.Where(employee =>
                    employee.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    employee.LatinFullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    employee.Position.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    employee.ContactNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    employee.Address.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    employee.Department?.KhmerName.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (SelectedDepartmentFilterId.HasValue)
            {
                query = query.Where(employee => employee.DepartmentId == SelectedDepartmentFilterId.Value);
            }

            if (SelectedActiveFilter.HasValue)
            {
                query = query.Where(employee => employee.IsActive == SelectedActiveFilter.Value);
            }

            List<Employee> filteredEmployees = query
                .OrderBy(employee => employee.FullName)
                .ToList();

            await _dispatcherService.InvokeAsync(() =>
            {
                TotalCount = filteredEmployees.Count;
                MaxPage = Math.Max(1, (int)Math.Ceiling((double)TotalCount / DefaultPageSize));

                if (CurrentPage > MaxPage)
                {
                    CurrentPage = MaxPage;
                    return;
                }
            });

            IEnumerable<Employee> pageEmployees = filteredEmployees
                .Skip((CurrentPage - 1) * DefaultPageSize)
                .Take(DefaultPageSize);

            await _dispatcherService.InvokeAsync(() =>
            {
                Employees.Clear();
                foreach (Employee employee in pageEmployees)
                {
                    Employees.Add(employee);
                }

                CurrentPageTotalCount = Employees.Count;
                PageCount = $"ទំព័រទី {CurrentPage} នៃ {MaxPage}";
            });
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < MaxPage)
            {
                CurrentPage++;
            }
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        [RelayCommand]
        private async Task RefreshOnFilterAsync()
        {
            CurrentPage = 1;
            await ApplyFiltersAndPaging();
        }
    }
}
