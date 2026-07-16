using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Employees.ViewModels
{
    public partial class EditEmployeeViewModel : EmployeeFormViewModelBase, INavigationAware
    {
        private readonly IEmployeeService _employeeService;

        private Employee? _employee;

        [ObservableProperty]
        private string _employeeName = string.Empty;

        public EditEmployeeViewModel(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService,
            IPhotoUploadService photoUploadService,
            IPhotoDeleteService photoDeleteService,
            IPhotoFetchService photoFetchService,
            IFileDialogService fileDialogService)
            : base(
                departmentService,
                authorizationService,
                messageService,
                navigationService,
                photoUploadService,
                photoDeleteService,
                photoFetchService,
                fileDialogService)
        {
            _employeeService = employeeService;
        }

        public override async Task LoadAsync()
        {
            if (_employee == null)
            {
                return;
            }

            IsLoading = true;

            try
            {
                await LoadCommonAsync();
                if (!CanManageEmployees)
                {
                    return;
                }
                LoadEmployeeToForm(_employee);
                EmployeeName = _employee.FullName;
                await LoadEmployeePhotoAsync(_employee.PhotoKey);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is not EditEmployeeParams p || p.Employee == null)
            {
                return;
            }

            _employee = p.Employee;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!CanManageEmployees || _employee == null)
            {
                return;
            }

            if (!ValidateRequiredFields())
            {
                return;
            }

            IsLoading = true;

            try
            {
                Employee employee = BuildEmployeeFromForm();
                employee.Id = _employee.Id;

                ReturnResponse response = await _employeeService.UpdateAsync(employee);
                if (response.Status != Status.Success)
                {
                    _messageService.Show(response.Message ?? "Unable to save the employee.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                await UploadEmployeePhotoAsync(employee);
                await DeleteEmployeePhotoAsync(employee);

                _messageService.Show("Employee updated successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while updating the employee: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await GoBackAsync();
        }
    }
}
