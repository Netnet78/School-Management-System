using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Employees.Enums;
using SchoolManagement.Core.Features.Files.Models;
using SchoolManagement.Core.Shared.Extensions;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Employees.ViewModels
{
    public abstract partial class EmployeeFormViewModelBase : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IDepartmentService _departmentService;
        private readonly IAuthorizationService _authorizationService;
        protected readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IPhotoUploadService _photoUploadService;
        private readonly IPhotoDeleteService _photoDeleteService;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IFileDialogService _fileDialogService;

        private int _editingEmployeeId;
        private string? _existingPhotoKey;
        private bool _photoChanged;

        [ObservableProperty]
        private ObservableCollection<Department?> _departments = [];

        [ObservableProperty]
        private bool _canManageEmployees;

        [ObservableProperty]
        private bool _canChooseDepartment;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _latinFullName = string.Empty;

        [ObservableProperty]
        private string _position = string.Empty;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private DateOnly? _hiredDate = DateOnly.FromDateTime(DateTime.Today);

        [ObservableProperty]
        private Gender _gender = Gender.Male;

        [ObservableProperty]
        private DateOnly? _dateOfBirth = DateOnly.FromDateTime(DateTime.Today);

        [ObservableProperty]
        private string _placeOfBirth = string.Empty;

        [ObservableProperty]
        private string _contactNumber = string.Empty;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private MaritalStatus _maritalStatus = MaritalStatus.Single;

        [ObservableProperty]
        private int? _departmentId;

        [ObservableProperty]
        private DateOnly? _salaryDate = DateOnly.FromDateTime(DateTime.Today);

        [ObservableProperty]
        private decimal _baseSalary;

        [ObservableProperty]
        private decimal _bonus;

        [ObservableProperty]
        private decimal _deduction;

        [ObservableProperty]
        private decimal _tax;

        [ObservableProperty]
        private string? _currentPhoto;

        public IEnumerable<object> GenderOptions { get; } = Enum.GetValues<Gender>()
            .Select(g => new { Value = g, Description = g.GetDescription() });

        public IEnumerable<object> MaritalStatusOptions { get; } = Enum.GetValues<MaritalStatus>()
            .Select(s => new { Value = s, Description = s.GetDescription() });

        protected EmployeeFormViewModelBase(
            IDepartmentService departmentService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService,
            IPhotoUploadService photoUploadService,
            IPhotoDeleteService photoDeleteService,
            IPhotoFetchService photoFetchService,
            IFileDialogService fileDialogService)
        {
            _departmentService = departmentService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
            _photoUploadService = photoUploadService;
            _photoDeleteService = photoDeleteService;
            _photoFetchService = photoFetchService;
            _fileDialogService = fileDialogService;
        }

        public abstract Task LoadAsync();

        [RelayCommand]
        protected async Task UploadPhotoAsync()
        {
            FileDialogObject file = _fileDialogService.ShowDialog(
                "Select a Photo",
                false,
                "Image files",
                "png",
                "jpg",
                "jpeg",
                "bmp",
                "gif");

            if (file.File != null)
            {
                CurrentPhoto = file.GetFilePath();
                _photoChanged = true;
            }
        }

        [RelayCommand]
        protected Task DeletePhotoAsync()
        {
            CurrentPhoto = null;
            _photoChanged = true;
            return Task.CompletedTask;
        }

        protected async Task LoadCommonAsync()
        {
            User? currentUser = _authorizationService.CurrentUser;

            if (currentUser == null)
            {
                CanManageEmployees = false;
                _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            ReturnResponse permission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageEmployees);

            if (permission.Status != Status.Success)
            {
                CanManageEmployees = false;
                _messageService.Show(permission.Message ?? "You do not have permission to manage employees.", "Access denied", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            CanManageEmployees = true;

            CanChooseDepartment = currentUser.IsAdmin();

            if (!CanChooseDepartment && currentUser.Employee?.DepartmentId == null)
            {
                _messageService.Show("Unable to determine the current user's department.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            ReturnResponse<IEnumerable<Department>> response = CanChooseDepartment
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
                Departments.Add(new Department { Id = 0, KhmerName = "គ្មានផ្នែក", Name = "No department" });
                foreach (Department department in response.Value)
                {
                    Departments.Add(department);
                }
            }

            if (!CanChooseDepartment)
            {
                DepartmentId = currentUser.Employee?.DepartmentId;
            }
        }

        protected void ResetForm(bool keepDepartment = false)
        {
            User? currentUser = _authorizationService.CurrentUser;

            _editingEmployeeId = 0;
            _existingPhotoKey = null;
            _photoChanged = false;

            FullName = string.Empty;
            LatinFullName = string.Empty;
            Position = string.Empty;
            IsActive = true;
            HiredDate = DateOnly.FromDateTime(DateTime.Today);
            Gender = Gender.Male;
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today);
            PlaceOfBirth = string.Empty;
            ContactNumber = string.Empty;
            Address = string.Empty;
            MaritalStatus = MaritalStatus.Single;
            SalaryDate = DateOnly.FromDateTime(DateTime.Today);
            BaseSalary = 0;
            Bonus = 0;
            Deduction = 0;
            Tax = 0;
            CurrentPhoto = null;

            if (!keepDepartment)
            {
                DepartmentId = CanChooseDepartment ? 0 : currentUser?.Employee?.DepartmentId;
            }
        }

        protected void LoadEmployeeToForm(Employee employee)
        {
            _editingEmployeeId = employee.Id;
            _existingPhotoKey = employee.PhotoKey;
            _photoChanged = false;

            FullName = employee.FullName;
            LatinFullName = employee.LatinFullName;
            Position = employee.Position;
            IsActive = employee.IsActive;
            HiredDate = employee.HiredDate;
            Gender = employee.Gender;
            DateOfBirth = employee.DateOfBirth;
            PlaceOfBirth = employee.PlaceOfBirth;
            ContactNumber = employee.ContactNumber;
            Address = employee.Address;
            MaritalStatus = employee.MaritalStatus;
            DepartmentId = employee.DepartmentId;
            SalaryDate = employee.SalaryDate;
            BaseSalary = employee.BaseSalary;
            Bonus = employee.Bonus;
            Deduction = employee.Deduction;
            Tax = employee.Tax;
        }

        protected Employee BuildEmployeeFromForm()
        {
            User? currentUser = _authorizationService.CurrentUser;

            return new Employee
            {
                Id = _editingEmployeeId,
                FullName = FullName.Trim(),
                LatinFullName = LatinFullName.Trim(),
                Position = Position.Trim(),
                IsActive = IsActive,
                HiredDate = HiredDate ?? DateOnly.FromDateTime(DateTime.Today),
                Gender = Gender,
                DateOfBirth = DateOfBirth ?? DateOnly.FromDateTime(DateTime.Today),
                PlaceOfBirth = PlaceOfBirth.Trim(),
                ContactNumber = ContactNumber.Trim(),
                Address = Address.Trim(),
                MaritalStatus = MaritalStatus,
                DepartmentId = CanChooseDepartment ? (DepartmentId == 0 ? null : DepartmentId) : currentUser?.Employee?.DepartmentId,
                SalaryDate = SalaryDate ?? DateOnly.FromDateTime(DateTime.Today),
                BaseSalary = BaseSalary,
                Bonus = Bonus,
                Deduction = Deduction,
                Tax = Tax
            };
        }

        protected bool ValidateRequiredFields()
        {
            User? currentUser = _authorizationService.CurrentUser;

            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(LatinFullName) ||
                string.IsNullOrWhiteSpace(Position) ||
                string.IsNullOrWhiteSpace(PlaceOfBirth) ||
                string.IsNullOrWhiteSpace(ContactNumber) ||
                string.IsNullOrWhiteSpace(Address))
            {
                _messageService.Show(
                    "សូមមេត្តាបំពេញព័ត៌មានឱ្យបានពេញលេញ មុននឹងបន្តទៅមុខ!",
                    "ខ្វះព័ត៌មាន!",
                    MessageButton.OK,
                    MessageIcon.Information);
                return false;
            }



            if (currentUser == null)
            {
                _messageService.Show("សូមធ្វើការ login ចូលប្រព័ន្ធមួយនេះដើម្បីបន្តការប្រើប្រាស់.", "ឈប់សិន!", MessageButton.OK, MessageIcon.Error);
                return false;
            }

            return true;
        }

        protected async Task LoadEmployeePhotoAsync(string? photoKey)
        {
            CurrentPhoto = null;

            if (string.IsNullOrWhiteSpace(photoKey))
            {
                return;
            }

            ReturnResponse<FileObject> response = await _photoFetchService.GetEmployeePhoto(photoKey);
            if (response.Status == Status.Success)
            {
                CurrentPhoto = response.Value?.FilePath;
            }
        }

        protected async Task UploadEmployeePhotoAsync(Employee employee)
        {
            if (!_photoChanged || string.IsNullOrWhiteSpace(CurrentPhoto))
            {
                return;
            }

            ReturnResponse<FileObject> uploadResponse = await _photoUploadService.UploadEmployeePhoto(CurrentPhoto, employee);
            if (uploadResponse.Status == Status.Failed)
            {
                _messageService.Show(
                    uploadResponse.Message ?? "The employee photo could not be uploaded.",
                    "Photo upload warning",
                    MessageButton.OK,
                    MessageIcon.Exclamation);
            }

            _photoChanged = false;
        }

        protected async Task DeleteEmployeePhotoAsync(Employee employee)
        {
            if (!_photoChanged)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_existingPhotoKey) || CurrentPhoto != null)
            {
                return;
            }

            await _photoDeleteService.DeleteEmployeePhoto(employee);
            _existingPhotoKey = null;
            _photoChanged = false;
        }

        protected async Task GoBackAsync()
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
