using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Generations.Models;
using SchoolManagement.Core.Features.Grades.Models;
using SchoolManagement.Presentation.Features.Subjects.ViewModels;
using SchoolManagement.Presentation.Shared.Features.Subjects.Params;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Classes.ViewModels
{
    public partial class AddClassViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDepartmentService _departmentService;
        private readonly IGradeService _gradeService;
        private readonly IGenerationService _generationService;
        private readonly IEmployeeService _employeeService;

        [ObservableProperty]
        private Grade? _selectedGrade;

        [ObservableProperty]
        private Generation? _selectedGeneration;

        [ObservableProperty]
        private Employee? _selectedTeacher;

        [ObservableProperty]
        private Department? _selectedDepartment;

        [ObservableProperty]
        private ObservableCollection<Grade> _grades = [];

        [ObservableProperty]
        private ObservableCollection<Generation> _generations = [];

        [ObservableProperty]
        private ObservableCollection<Employee> _teachers = [];

        [ObservableProperty]
        private ObservableCollection<Department> _departmentsForForm = [];

        [ObservableProperty]
        private bool _isAdmin;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _showGenerationForm;

        [ObservableProperty]
        private int _cohortNumber;

        [ObservableProperty]
        private int _academicStartYear;

        [ObservableProperty]
        private int _academicEndYear;

        public AddClassViewModel(
            IClassService classService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService,
            IDepartmentService departmentService,
            IGradeService gradeService,
            IGenerationService generationService,
            IEmployeeService employeeService)
        {
            _classService = classService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
            _departmentService = departmentService;
            _gradeService = gradeService;
            _generationService = generationService;
            _employeeService = employeeService;
        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            try
            {
                User? user = _authorizationService.CurrentUser;
                if (user == null)
                {
                    _messageService.Show("Current user session hasn't been set!", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }
                IsAdmin = user.IsAdmin();

                var gradesResponse = await _gradeService.GetAllAsync(1);
                if (gradesResponse.Status == Status.Success && gradesResponse.Value != null)
                {
                    Grades = new ObservableCollection<Grade>(gradesResponse.Value);
                }

                var teachersResponse = await _employeeService.GetAllAsync(1);
                if (teachersResponse.Status == Status.Success && teachersResponse.Value != null)
                {
                    Teachers = new ObservableCollection<Employee>(teachersResponse.Value);
                }

                if (IsAdmin)
                {
                    var deptResponse = await _departmentService.GetAllAsync(1);
                    if (deptResponse.Status == Status.Success && deptResponse.Value != null)
                    {
                        DepartmentsForForm = new ObservableCollection<Department>(deptResponse.Value);
                    }
                }
                else
                {
                    int? departmentId = user.Employee?.DepartmentId;
                    if (departmentId.HasValue)
                    {
                        var genResponse = await _generationService.GetAllAsync(
                            filters: [new FilterCondition<Generation>(g => g.DepartmentId, FilterOperator.Equals, departmentId.Value)]);
                        if (genResponse.Status == Status.Success && genResponse.Value != null)
                        {
                            Generations = new ObservableCollection<Generation>(genResponse.Value);
                        }
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        async partial void OnSelectedDepartmentChanged(Department? value)
        {
            if (value == null) return;

            ShowGenerationForm = false;
            SelectedGeneration = null;
            Generations.Clear();

            var genResponse = await _generationService.GetAllAsync(
                filters: [new FilterCondition<Generation>(g => g.DepartmentId, FilterOperator.Equals, value.Id)]);
            if (genResponse.Status == Status.Success && genResponse.Value != null)
            {
                Generations = new ObservableCollection<Generation>(genResponse.Value);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedGrade == null)
            {
                _messageService.Show("សូមជ្រើសរើសកម្រិត!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (SelectedGeneration == null)
            {
                _messageService.Show("សូមជ្រើសរើសជំនាន់!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                Class cls = new()
                {
                    GradeId = SelectedGrade.Id,
                    GenerationId = SelectedGeneration.Id,
                    TeacherId = SelectedTeacher?.Id
                };

                var response = await _classService.InsertAsync(cls);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានបន្ថែមថ្នាក់ថ្មីដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);

                    // Navigate to subject assignment for the newly created class
                    await _navigationService.NavigateAsync<SubjectAssignmentViewModel>(
                        new SubjectAssignmentParams { Class = cls });
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
        private async Task CancelAsync()
        {
            await GoBack();
        }

        [RelayCommand]
        private void AddGeneration()
        {
            CohortNumber = 0;
            AcademicStartYear = DateTime.Now.Year;
            AcademicEndYear = DateTime.Now.Year + 1;
            ShowGenerationForm = true;
        }

        [RelayCommand]
        private async Task SaveGenerationAsync()
        {
            if (CohortNumber <= 0)
            {
                _messageService.Show("សូមបញ្ចូលលេខជំនាន់!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            int departmentId;
            if (IsAdmin)
            {
                if (SelectedDepartment == null)
                {
                    _messageService.Show("សូមជ្រើសរើសផ្នែក!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                    return;
                }
                departmentId = SelectedDepartment.Id;
            }
            else
            {
                User? user = _authorizationService.CurrentUser;
                if (user == null)
                {
                    _messageService.Show("Current user session hasn't been set!", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }
                if (user.Employee?.DepartmentId == null)
                {
                    _messageService.Show("មិនអាចកំណត់ផ្នែកបានទេ ពីព្រោះបុគ្គលិកគ្មានទិន្នន័យនៅផ្នែកណាឡើយ!", "ឈប់សិន!", MessageButton.OK, MessageIcon.Error);
                    return;
                }
                departmentId = user.Employee.DepartmentId.Value;
            }

            IsLoading = true;

            try
            {
                Generation gen = new()
                {
                    CohortNumber = CohortNumber,
                    AcademicStartYear = AcademicStartYear,
                    AcademicEndYear = AcademicEndYear,
                    DepartmentId = departmentId
                };

                var response = await _generationService.InsertAsync(gen);

                if (response.Status == Status.Success)
                {
                    ShowGenerationForm = false;

                    var genResponse = await _generationService.GetAllAsync(
                        filters: [new FilterCondition<Generation>(g => g.DepartmentId, FilterOperator.Equals, departmentId)]);
                    if (genResponse.Status == Status.Success && genResponse.Value != null)
                    {
                        Generations = new ObservableCollection<Generation>(genResponse.Value);
                        SelectedGeneration = Generations.FirstOrDefault(g => g.Id == gen.Id);
                    }

                    _messageService.Show("បានបន្ថែមជំនាន់ថ្មីដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសក្នុងការរក្សាទុក!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មាកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CancelGeneration()
        {
            ShowGenerationForm = false;
        }

        private async Task GoBack()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;
            if (previous != null)
            {
                await _navigationService.NavigateAsync(previous.GetType());
            }
            else
            {
                await _navigationService.NavigateAsync<ClassViewModel>();
            }
        }
    }
}
