using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Departments.Models;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Departments.ViewModels
{
    public partial class DepartmentViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IDepartmentService _departmentService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IAuthorizationService _authorizationService;

        [ObservableProperty]
        private ObservableCollection<Department> _departments = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _showDepartmentForm;

        [ObservableProperty]
        private bool _isEditingDepartment;

        [ObservableProperty]
        private string _departmentFormTitle = string.Empty;

        [ObservableProperty]
        private string _departmentName = string.Empty;

        [ObservableProperty]
        private string _departmentKhmerName = string.Empty;

        [ObservableProperty]
        private string _departmentDescription = string.Empty;

        private int _editingDepartmentId;

        public DepartmentViewModel(
            IDepartmentService departmentService,
            IMessageService messageService,
            INavigationService navigationService,
            IAuthorizationService authorizationService)
        {
            _departmentService = departmentService;
            _messageService = messageService;
            _navigationService = navigationService;
            _authorizationService = authorizationService;
        }

        public async Task LoadAsync()
        {
            await LoadDepartmentsAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            IsLoading = true;

            try
            {
                var response = await _departmentService.GetAllAsync(1);
                if (response.Status == Status.Success && response.Value != null)
                {
                    Departments.Clear();
                    foreach (Department dept in response.Value)
                    {
                        Departments.Add(dept);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AddDepartment()
        {
            IsEditingDepartment = false;
            DepartmentFormTitle = "បន្ថែមផ្នែកថ្មី";
            DepartmentName = string.Empty;
            DepartmentKhmerName = string.Empty;
            DepartmentDescription = string.Empty;
            ShowDepartmentForm = true;
        }

        [RelayCommand]
        private void EditDepartment(Department? dept)
        {
            if (dept == null) return;

            IsEditingDepartment = true;
            DepartmentFormTitle = "កែប្រែផ្នែក";
            _editingDepartmentId = dept.Id;
            DepartmentName = dept.Name;
            DepartmentKhmerName = dept.KhmerName;
            DepartmentDescription = dept.Description;
            ShowDepartmentForm = true;
        }

        [RelayCommand]
        private async Task SaveDepartmentAsync()
        {
            if (string.IsNullOrWhiteSpace(DepartmentName) || string.IsNullOrWhiteSpace(DepartmentKhmerName))
            {
                _messageService.Show("សូមបំពេញឈ្មោះ និងឈ្មោះជាអក្សរខ្មែរ!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                Department dept = new()
                {
                    Name = DepartmentName,
                    KhmerName = DepartmentKhmerName,
                    Description = DepartmentDescription,
                    IsActive = true
                };

                ReturnResponse response;

                if (IsEditingDepartment)
                {
                    dept.Id = _editingDepartmentId;
                    response = await _departmentService.UpdateAsync(dept);
                }
                else
                {
                    response = await _departmentService.InsertAsync(dept);
                }

                if (response.Status == Status.Success)
                {
                    _messageService.Show(
                        IsEditingDepartment ? "បានកែប្រែផ្នែកដោយជោគជ័យ!" : "បានបន្ថែមផ្នែកថ្មីដោយជោគជ័យ!",
                        "ជោគជ័យ",
                        MessageButton.OK,
                        MessageIcon.Success);

                    ShowDepartmentForm = false;
                    await LoadDepartmentsAsync();
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសក្នុងការរក្សាទុក!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CancelDepartmentForm()
        {
            ShowDepartmentForm = false;
        }

        [RelayCommand]
        private async Task DeleteDepartmentAsync(Department? dept)
        {
            if (dept == null) return;

            MessageResult result = _messageService.Show(
                $"តើអ្នកប្រាកដទេថានឹងលុបផ្នែក \"{dept.KhmerName}\"?",
                "បញ្ជាក់ការលុប",
                MessageButton.YesNo,
                MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            IsLoading = true;

            try
            {
                ReturnResponse response = await _departmentService.DeleteAsync(dept);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានលុបផ្នែកដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                    Departments.Remove(dept);
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសក្នុងការលុប!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
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
        }
    }
}
