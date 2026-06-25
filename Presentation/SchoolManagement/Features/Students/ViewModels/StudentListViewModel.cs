using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Generations.Models;
using SchoolManagement.Presentation.Shared.Features.Students.Observables;
using SchoolManagement.Presentation.Shared.Features.Students.Params;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public partial class StudentListViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IStudentService _studentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IDepartmentService _departmentService;
        private readonly IGenerationService _generationService;

        private const int DefaultPageSize = 10;

        private CancellationTokenSource? _cts;
        private bool _isInitializing;

        public StudentListViewModel(
            IStudentService studentService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService,
            IDepartmentService departmentService,
            IGenerationService generationService,
            IDispatcherService dispatcherService)
        {
            _studentService = studentService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
            _departmentService = departmentService;
            _generationService = generationService;

            Students = new();
            Departments = new();
            Generations = new();

            Filters.PropertyChanged += OnFilterPropertyChanged;
            _dispatcherService = dispatcherService;
        }

        public StudentFilterObservableModel Filters { get; } = new();

        [ObservableProperty]
        private ObservableCollection<Student> _students;


        [ObservableProperty]
        private bool _isLoading;

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

        [ObservableProperty]
        private bool _canViewStudents;

        [ObservableProperty]
        private bool _canInsertStudents;

        [ObservableProperty]
        private bool _canEditStudents;

        [ObservableProperty]
        private bool _canDeleteStudents;

        [ObservableProperty]
        private bool _canAssignClass;

        [ObservableProperty]
        private bool _isAdmin;

        [ObservableProperty]
        private ObservableCollection<Department> _departments;

        [ObservableProperty]
        private ObservableCollection<Generation> _generations;

        [ObservableProperty]
        private Department? _selectedDepartment;

        [ObservableProperty]
        private Generation? _selectedGeneration;

        public bool HasSelectedDepartment => SelectedDepartment != null;

        [RelayCommand]
        private async Task LoadStudentsAsync()
        {
            IsLoading = true;

            try
            {
                List<FilterCondition<Student>> filters = Filters.BuildFilters();
                IEnumerable<SortCriteria<Student>> order = Filters.BuildOrder();

                var response = await _studentService.GetAllAsync(CurrentPage, DefaultPageSize, filters, order, "Candidate", "Candidate.Skill", "Candidate.Photo");

                if (response.Status == Status.Success && response.Value != null)
                {
                    Students.Clear();
                    foreach (var student in response.Value)
                    {
                        Students.Add(student);
                    }

                    var countResponse = await _studentService.GetAllCountAsync(1, null, filters);

                    if (countResponse.Status == Status.Success)
                    {
                        TotalCount = countResponse.Value;
                    }

                    var currentPageCountResponse = await _studentService.GetAllCountAsync(CurrentPage, DefaultPageSize, filters);

                    if (countResponse.Status == Status.Success)
                    {
                        CurrentPageTotalCount = currentPageCountResponse.Value;
                    }

                    double pages = Math.Ceiling((double)TotalCount / DefaultPageSize);
                    MaxPage = (int)pages;
                    PageCount = $"ទំព័រទី {CurrentPage} នៃ {pages:F0}";
                }
                else
                {
                    _messageService.Show(response.Message ?? "Failed to load students");
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading students: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _messageService.Show($"{ex.InnerException.Message}");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddStudentAsync()
        {
            await _navigationService.NavigateAsync<AddStudentOptionViewModel>();
        }

        [RelayCommand]
        private async Task AssignClassAsync(Student student)
        {
            if (student == null) return;

            await _navigationService.NavigateAsync<AssignStudentClassViewModel>(new AssignStudentClassParams { Student = student });
        }

        [RelayCommand]
        private async Task EditStudentAsync(Student student)
        {
            await _navigationService.NavigateAsync<EditStudentViewModel>(new EditStudentNavigationParams { Student = student });
        }

        [RelayCommand]
        private async Task DeleteStudentAsync(Student student)
        {
            MessageResult result = _messageService.Show("តើអ្នកចង់លុបទិន្នន័យសិស្សចេញពី Database ឬក៏បញ្ឈប់សិស្សនិងកំណត់ជាសិស្សត្រូវបានឈប់រៀន?" +
                "\n1. សូមចុច 'Yes' ដើម្បីលុបទាំងស្រុង" +
                "\n2. សូមចុច 'No' ដើម្បីដាក់សិស្សជាសិស្សឈប់រៀន (ទិន្នន័យមិនត្រូវលុបទេ)" +
                "\n3. សូមចុច 'Cancel' ដើម្បីត្រឡប់ទៅក្រោយវិញ", "ឈប់សិន!", MessageButton.YesNoCancel, MessageIcon.Information);

            if (result == MessageResult.Yes)
            {
                await _studentService.DeleteAsync(student);
                await LoadStudentsCommand.ExecuteAsync(null);
                return;
            }

            if (result == MessageResult.No)
            {
                student.IsActive = false;
                await _studentService.UpdateAsync(student);
                await LoadStudentsCommand.ExecuteAsync(null);
                return;
            }
        }

        [RelayCommand]
        private async Task CheckPermissions()
        {
            try
            {
                CanViewStudents = (await _authorizationService.AuthorizeAsync(null, PermissionType.ViewStudents)).Status == Status.Success;
                CanInsertStudents = (await _authorizationService.AuthorizeAsync(null, PermissionType.InsertStudents)).Status == Status.Success;
                CanEditStudents = (await _authorizationService.AuthorizeAsync(null, PermissionType.EditStudents)).Status == Status.Success;
                CanDeleteStudents = (await _authorizationService.AuthorizeAsync(null, PermissionType.DeleteStudents)).Status == Status.Success;
                CanAssignClass = (await _authorizationService.AuthorizeAsync(null, PermissionType.EditStudents)).Status == Status.Success;
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while checking permissions: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NextPage()
        {
            if (CurrentPage < MaxPage)
            {
                CurrentPage++;
            }
        }

        [RelayCommand]
        private async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        async partial void OnCurrentPageChanged(int oldValue, int newValue)
        {
            if (IsLoading) return;

            if (newValue > MaxPage)
            {
                CurrentPage = oldValue;
                return;
            }

            if (newValue < 1)
            {
                CurrentPage = oldValue;
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                await Task.Delay(1000, token);
                await LoadStudentsCommand.ExecuteAsync(null);
            }
            catch (TaskCanceledException) { }
        }

        async partial void OnSelectedDepartmentChanged(Department? value)
        {
            _dispatcherService.Invoke(() =>
            {
                Filters.DepartmentId = value?.Id;
                Generations.Clear();
                SelectedGeneration = null;
                Filters.GenerationId = null;
                OnPropertyChanged(nameof(HasSelectedDepartment));
            });

            if (value == null) return;

            await Task.Delay(300);

            var response = await _generationService.GetAllAsync(
                filters: [new FilterCondition<Generation>(g => g.DepartmentId, FilterOperator.Equals, value.Id)],
                orderBy: [new SortCriteria<Generation>("CohortNumber", OrderDirection.Descending)],
                includes: ["Department"]);
            if (response.Status == Status.Success && response.Value != null)
            {
                _dispatcherService.Invoke(() =>
                {
                    foreach (var gen in response.Value)
                    {
                        Generations.Add(gen);
                    }
                });
            }
        }

        partial void OnSelectedGenerationChanged(Generation? value)
        {
            Filters.GenerationId = value?.Id;
        }

        private async void OnFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInitializing) return;

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

        [RelayCommand]
        private async Task ResetFilters()
        {
            Filters.Search = null;
            Filters.Gender = null;
            Filters.SortBy = null;
            Filters.OrderBy = OrderDirection.Descending;
            Filters.FromDate = null;
            Filters.ToDate = null;
            Filters.StayType = null;

            if (IsAdmin)
            {
                Filters.DepartmentId = null;
                Filters.GenerationId = null;
                SelectedDepartment = null;
                SelectedGeneration = null;
                Generations.Clear();
            }

            await RefreshOnFilterAsync();
        }

        [RelayCommand]
        private async Task RefreshOnFilterAsync()
        {
            CurrentPage = 1;
            
            if (!_isInitializing)
            {
                await LoadStudentsAsync();
            }
        }

        public async Task LoadAsync()
        {
            if (IsLoading || _isInitializing) return;
            _isInitializing = true;

            try
            {
                User? currentUser = _authorizationService.CurrentUser;
                if (currentUser == null)
                {
                    _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                IsAdmin = currentUser.IsAdmin();

                await CheckPermissions();

                if (!CanViewStudents)
                {
                    _messageService.Show("អ្នកគ្មានសិទ្ធិមើលទិន្នន័យតារាងសិស្សនោះទេ!", "ឈប់សិន!", MessageButton.OK, MessageIcon.Hand);
                    return;
                }

                var deptResponse = await _departmentService.GetAllAsync(1, null);
                if (deptResponse.Status == Status.Success && deptResponse.Value != null)
                {
                    Departments.Clear();
                    foreach (var dept in deptResponse.Value)
                    {
                        Departments.Add(dept);
                    }
                }

                if (!currentUser.IsAdmin())
                {
                    var selected = Departments.Where(d => d.Id == currentUser.Employee?.DepartmentId).FirstOrDefault();
                    SelectedDepartment = selected;
                }

                await LoadStudentsAsync();
            }
            finally
            {
                _isInitializing = false;
            }
        }
    }
}
