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
    public partial class EditClassViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDepartmentService _departmentService;
        private readonly IGradeService _gradeService;
        private readonly IGenerationService _generationService;
        private readonly IEmployeeService _employeeService;

        private Class _class = null!;
        private bool _isInitializing;

        [ObservableProperty]
        private string _className = string.Empty;

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

        public EditClassViewModel(
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

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is not EditClassParams p || p.Class == null)
            {
                _messageService.Show("មិនអាចរកឃើញទិន្នន័យថ្នាក់ដែលត្រូវកែប្រែនោះទេ!", "មានកំហុស!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            var response = await _classService.GetByIdWithSubjectsAsync(p.Class.Id);
            if (response.Status != Status.Success || response.Value == null)
            {
                _messageService.Show(response.Message ?? "Class data could not be loaded.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            _class = response.Value;
        }

        public async Task LoadAsync()
        {
            if (_class == null)
            {
                _messageService.Show("Class cannot be null!", icon: MessageIcon.Error);
                return;
            }

            IsLoading = true;
            _isInitializing = true;

            try
            {
                Class cls = _class;
                ClassName = cls.GetKhmerName();

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
                    SelectedGrade = Grades.FirstOrDefault(g => g.Id == cls.GradeId) ?? cls.Grade;
                }

                var teachersResponse = await _employeeService.GetAllAsync(1);
                if (teachersResponse.Status == Status.Success && teachersResponse.Value != null)
                {
                    Teachers = new ObservableCollection<Employee>(teachersResponse.Value);
                    SelectedTeacher = Teachers.FirstOrDefault(t => t.Id == cls.TeacherId);
                }

                if (IsAdmin)
                {
                    var deptResponse = await _departmentService.GetAllAsync(1);
                    if (deptResponse.Status == Status.Success && deptResponse.Value != null)
                    {
                        DepartmentsForForm = new ObservableCollection<Department>(deptResponse.Value);
                        SelectedDepartment = DepartmentsForForm.FirstOrDefault(d => d.Id == cls.Generation.DepartmentId)
                            ?? cls.Generation.Department;
                    }

                    if (SelectedDepartment != null)
                    {
                        var genResponse = await _generationService.GetAllAsync(
                            filters: [new FilterCondition<Generation>(g => g.DepartmentId, FilterOperator.Equals, SelectedDepartment.Id)]);
                        if (genResponse.Status == Status.Success && genResponse.Value != null)
                        {
                            Generations = new ObservableCollection<Generation>(genResponse.Value);
                        }
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

                    SelectedDepartment = cls.Generation.Department;
                }

                SelectedGeneration = Generations.FirstOrDefault(g => g.Id == cls.GenerationId) ?? cls.Generation;
            }
            finally
            {
                _isInitializing = false;
                IsLoading = false;
            }
        }

        async partial void OnSelectedDepartmentChanged(Department? value)
        {
            if (value == null || _isInitializing) return;

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
                _messageService.Show("សូមជ្រើសរើសកម្រិតសិន!", "ព័ត៌មានមិនគ្រប់គ្រាន់!", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (SelectedGeneration == null)
            {
                _messageService.Show("សូមជ្រើសរើសជំនាន់សិន!", "ព័ត៌មានមិនគ្រប់គ្រាន់!", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                Class cls = _class;
                cls.GradeId = SelectedGrade.Id;
                cls.GenerationId = SelectedGeneration.Id;
                cls.TeacherId = SelectedTeacher?.Id;

                var response = await _classService.UpdateAsync(cls);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("ថ្នាក់រៀនត្រូវបានកែប្រែ​ដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                    await GoBack();
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
        private async Task ManageSubjectsAsync()
        {
            if (_class == null) return;

            await _navigationService.NavigateAsync<SubjectAssignmentViewModel>(
                new SubjectAssignmentParams { Class = _class });
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
                    _messageService.Show("មិនអាចកំណត់ផ្នែកបានទេ!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
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
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
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
