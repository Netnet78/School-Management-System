using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Generations.Models;
using SchoolManagement.Presentation.Shared.Features.Students.Observables;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SchoolManagement.Presentation.Features.Classes.ViewModels
{
    public partial class AddStudentsToClassViewModel : ObservableObject, IViewModel, INavigationAware, IAsyncLoadable
    {
        private readonly IStudentService _studentService;
        private readonly IStudentClassService _studentClassService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDepartmentService _departmentService;
        private readonly IGenerationService _generationService;
        private readonly IAuthorizationService _authorizationService;

        private Class? _class;
        private const int PageSize = 50;
        private List<Student>? _allFilteredStudents;
        private readonly SemaphoreSlim _loadLock = new(1, 1);
        private bool _isInitialized;
        private bool _isInitializing;
        private bool _suppressStudentReload;

        public StudentFilterObservableModel Filters { get; } = new();

        [ObservableProperty]
        private ObservableCollection<StudentCheckItem> _availableStudents = [];

        private HashSet<int> _selectedIds = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _maxPage = 1;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _pageCount = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Department> _departments = [];

        [ObservableProperty]
        private ObservableCollection<Generation> _generations = [];

        [ObservableProperty]
        private Department? _selectedDepartment;

        [ObservableProperty]
        private Generation? _selectedGeneration;

        [ObservableProperty]
        private bool _canSelectDepartment = false;

        public bool HasSelectedDepartment => SelectedDepartment != null;

        public AddStudentsToClassViewModel(
            IStudentService studentService,
            IStudentClassService studentClassService,
            IMessageService messageService,
            INavigationService navigationService,
            IDepartmentService departmentService,
            IGenerationService generationService,
            IAuthorizationService authorizationService)
        {
            _studentService = studentService;
            _studentClassService = studentClassService;
            _messageService = messageService;
            _navigationService = navigationService;
            _departmentService = departmentService;
            _generationService = generationService;
            _authorizationService = authorizationService;

            Filters.PropertyChanged += OnFiltersUpdated;
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is not AddStudentsToClassParams p) return;

            _class = p.Class;
            ClassName = _class?.GetKhmerName() ?? string.Empty;

            CanSelectDepartment = _authorizationService.CurrentUser?.IsValidRole(RoleType.Admin) == true;
        }

        public async Task LoadAsync()
        {
            if (_class == null)
            {
                _messageService.Show("ថ្នាក់រៀនមិនត្រូវបានជ្រើសរើសទេ!", "Error",
                    MessageButton.OK, MessageIcon.Error);
                return;
            }

            _isInitializing = true;
            try
            {
                if (CanSelectDepartment)
                {
                    ReturnResponse<IEnumerable<Department>> deptsResponse = await _departmentService.GetAllAsync(1, null, includes: "Generations");

                    if (deptsResponse.Status == Status.Success && deptsResponse.Value != null)
                    {
                        Departments.Clear();
                        foreach (Department dept in deptsResponse.Value)
                        {
                            Departments.Add(dept);
                        }
                    }
                }
                else
                {
                    User? user = _authorizationService.CurrentUser;
                    ReturnResponse<Department?> deptResponse = await _departmentService.GetByIdAsync(user?.Employee?.DepartmentId ?? 0);

                    if (deptResponse.Status == Status.Success && deptResponse.Value != null)
                    {
                        Departments.Clear();
                        Departments.Add(deptResponse.Value);

                        _suppressStudentReload = true;
                        try
                        {
                            SelectedDepartment = deptResponse.Value;
                            Filters.DepartmentId = deptResponse.Value.Id;
                            await LoadGenerationsForDepartment(deptResponse.Value.Id);
                        }
                        finally
                        {
                            _suppressStudentReload = false;
                        }
                    }
                }

                await LoadStudents();
            }
            finally
            {
                _isInitializing = false;
            }

            _isInitialized = true;
        }

        private async void OnFiltersUpdated(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInitializing || _suppressStudentReload) return;

            CurrentPage = 1;
            await LoadStudents();
        }

        private async Task LoadStudents()
        {
            if (_class == null) return;
            if (!await _loadLock.WaitAsync(0)) return;

            IsLoading = true;
            try
            {
                HashSet<int> existingStudentIds = await GetExistingStudentIds();

                List<FilterCondition<Student>> filters = Filters.BuildFilters();
                var orders = Filters.BuildOrder();

                var allResponse = await _studentService.GetAllAsync(1, null, filters, orders, "Candidate");
                if (allResponse.Status != Status.Success || allResponse.Value == null)
                    return;

                List<Student> filtered = allResponse.Value
                    .Where(s => !existingStudentIds.Contains(s.Id))
                    .ToList();

                _allFilteredStudents = filtered;
                TotalCount = filtered.Count;

                double pages = Math.Ceiling((double)TotalCount / PageSize);
                MaxPage = pages > 0 ? (int)pages : 1;
                PageCount = $"ទំព័រទី {CurrentPage} នៃ {pages:F0}";

                ApplyPagination();
            }
            catch (Exception ex)
            {
                _messageService.Show("មានបញ្ហាបច្ចេកទេស អំឡុងពេលដែលច្រោះទិន្នន័យសិស្សានុសិស្ស! សូមព្យាយាមម្ដងទៀតនៅពេលក្រោយ" +
                    $"\nReason: {ex.Message}",
                    "សូមព្យាយាមម្ដងទៀតនៅពេលក្រោយ!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }

        private async Task<HashSet<int>> GetExistingStudentIds()
        {
            HashSet<int> existingIds = [];
            if (_class == null) return existingIds;

            var existingResponse = await _studentClassService.GetAllAsync(
                filters: [new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, _class.Id)]);

            if (existingResponse.Status == Status.Success && existingResponse.Value != null)
            {
                foreach (var sc in existingResponse.Value)
                {
                    if (sc.StudentId > 0)
                        existingIds.Add(sc.StudentId);
                }
            }
            return existingIds;
        }

        private void ApplyPagination()
        {
            if (_allFilteredStudents == null) return;

            AvailableStudents.Clear();

            int offset = (CurrentPage - 1) * PageSize;
            var page = _allFilteredStudents.Skip(offset).Take(PageSize).ToList();

            foreach (Student student in page)
            {
                StudentCheckItem item = new() { Student = student };
                AttachStudentItem(item);
                AvailableStudents.Add(item);
            }
        }

        private void AttachStudentItem(StudentCheckItem item)
        {
            item.PropertyChanged += OnStudentCheckChanged;
            item.IsSelected = _selectedIds.Contains(item.Student?.Id ?? 0);
        }

        private void OnStudentCheckChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(StudentCheckItem.IsSelected)) return;
            if (sender is not StudentCheckItem item || item.Student == null) return;
            if (item.IsSelected)
                _selectedIds.Add(item.Student.Id);
            else
                _selectedIds.Remove(item.Student.Id);
        }

        [RelayCommand]
        private async Task NextPage()
        {
            if (CurrentPage < MaxPage)
            {
                CurrentPage++;
                ApplyPagination();
                await Task.CompletedTask;
            }
        }

        [RelayCommand]
        private async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                ApplyPagination();
                await Task.CompletedTask;
            }
        }

        [RelayCommand]
        private void ClearSelection()
        {
            _selectedIds.Clear();
            foreach (var item in AvailableStudents)
                item.IsSelected = false;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_class == null) return;

            IsLoading = true;
            try
            {
                if (_selectedIds.Count == 0)
                {
                    _messageService.Show("សូមជ្រើសរើសសិស្សយ៉ាងហោចណាស់ម្នាក់!", "ពុំមានសិស្សទេ",
                        MessageButton.OK, MessageIcon.Exclamation);
                    return;
                }

                int added = 0;
                foreach (int studentId in _selectedIds)
                {
                    if (studentId <= 0) continue;

                    DateTime now = DateTime.Now;

                    int schoolYear = now.Month >= 11
                        ? now.Year
                        : now.Year - 1;

                    DateOnly startDate = new(schoolYear, 11, 1);
                    DateOnly endDate = new(schoolYear + 1, 11, 1);

                    StudentClass enrollment = new()
                    {
                        StudentId = studentId,
                        ClassId = _class.Id,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsActive = true
                    };

                    ReturnResponse insertResponse = await _studentClassService.InsertAsync(enrollment);
                    if (insertResponse.Status == Status.Success)
                        added++;
                }

                _messageService.Show($"បានបន្ថែមសិស្សចំនួន {added} នាក់ ដោយជោគជ័យ!", "ជយោ!",
                    MessageButton.OK, MessageIcon.Success);

                await GoBack();
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានបញ្ហាក្នុងការរក្សាទុក: {ex.Message}", "កំហុស!",
                    MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            MessageResult result = _messageService.Show("តើអ្នកចង់បោះបង់មែនទេ? ការផ្លាស់ប្ដូរនឹងមិនត្រូវបានរក្សាទុកទេ។",
                "បញ្ជាក់", MessageButton.YesNo, MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            await GoBack();
        }

        [RelayCommand]
        private async Task RefreshOnFilter()
        {
            CurrentPage = 1;
            await LoadStudents().ConfigureAwait(false);
        }

        private async Task GoBack()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;
            if (previous != null)
                await _navigationService.NavigateAsync(previous.GetType());
            else
                await _navigationService.NavigateAsync<ClassViewModel>();
        }

        async partial void OnSelectedDepartmentChanged(Department? value)
        {
            if (value == null || _isInitializing) return;

            _suppressStudentReload = true;
            try
            {
                Filters.DepartmentId = value.Id;
                Generations.Clear();
                SelectedGeneration = null;
                Filters.GenerationId = null;
                OnPropertyChanged(nameof(HasSelectedDepartment));

                await LoadGenerationsForDepartment(value.Id);
            }
            finally
            {
                _suppressStudentReload = false;
            }

            await LoadStudents();
        }

        partial void OnSelectedGenerationChanged(Generation? value)
        {
            Filters.GenerationId = value?.Id;
        }

        private async Task LoadGenerationsForDepartment(int departmentId)
        {
            var response = await _generationService.GetAllAsync(
                filters: [new FilterCondition<Generation>(g => g.DepartmentId, FilterOperator.Equals, departmentId)],
                orderBy: [new SortCriteria<Generation>("CohortNumber", OrderDirection.Descending)]);
            if (response.Status == Status.Success && response.Value != null)
            {
                Generations.Clear();
                foreach (Generation gen in response.Value)
                {
                    Generations.Add(gen);
                }
            }
            else
            {
                _messageService.Show("មានបញ្ហាអំឡុងពេលដែលកំពុងទាញយកព័ត៌មានជំនាន់" +
                    $"\nMessage: {response.Message}", "ERROR", MessageButton.OK,
                    MessageIcon.Error);
            }
        }
    }
}
