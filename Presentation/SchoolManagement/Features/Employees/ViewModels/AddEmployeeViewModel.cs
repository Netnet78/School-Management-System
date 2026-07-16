using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Employees.ViewModels
{
    public partial class AddEmployeeViewModel : EmployeeFormViewModelBase
    {
        private readonly IEmployeeService _employeeService;

        public AddEmployeeViewModel(
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
            IsLoading = true;

            try
            {
                await LoadCommonAsync();
                if (!CanManageEmployees)
                {
                    return;
                }
                ResetForm();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!CanManageEmployees)
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
                ReturnResponse response = await _employeeService.InsertAsync(employee);

                if (response.Status != Status.Success)
                {
                    _messageService.Show(response.Message ?? "Unable to save the employee.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                await UploadEmployeePhotoAsync(employee);

                _messageService.Show("Employee added successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while adding the employee: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
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
