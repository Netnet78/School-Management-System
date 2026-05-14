using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public partial class StudentListViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IStudentService _studentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly User _currentUser;

        private const int DefaultPageSize = 10;

        private CancellationTokenSource? _cts;

        public StudentListViewModel(
            IStudentService studentService,
            IAuthorizationService authorizationService,
            IMessageService messageService)
        {
            _studentService = studentService;
            _authorizationService = authorizationService;
            _messageService = messageService;

            _currentUser = _authorizationService.CurrentUser;

            Filters = new()
            {
                IsActive = true,
            };
            Students = new();

            Filters.PropertyChanged += OnFiltersUpdated;
        }

        [ObservableProperty]
        private StudentFilterObservableModel _filters;

        [ObservableProperty]
        private bool _showInactiveStudents = false;

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
        private bool _isAdmin;

        [RelayCommand]
        private async Task LoadStudents()
        {
            IsLoading = true;

            try
            {
                if (_currentUser.IsAdmin())
                {
                    ShowInactiveStudents = true;
                }

                var response = await _studentService.GetStudentsAsync(CurrentPage, DefaultPageSize, Filters.ToFilterOptions());

                if (response.Status == Status.Success && response.Value != null)
                {
                    Students.Clear();
                    foreach (var student in response.Value)
                    {
                        Students.Add(student);
                    }

                    var countResponse = await _studentService.GetStudentsCount(1, int.MaxValue, Filters.ToFilterOptions());

                    if (countResponse.Status == Status.Success)
                    {
                        TotalCount = countResponse.Value;
                    }

                    var currentPageCountResponse = await _studentService.GetStudentsCount(CurrentPage, DefaultPageSize, Filters.ToFilterOptions());

                    if (countResponse.Status == Status.Success)
                    {
                        CurrentPageTotalCount = currentPageCountResponse.Value;
                    }

                    double pages = Math.Ceiling((double)TotalCount / DefaultPageSize);
                    MaxPage = (int)pages;
                    PageCount = $"??????? {CurrentPage} ?? {pages:F0}";
                }
                else
                {
                    _messageService.Show(response.Message ?? "Failed to load students");
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading students: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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

        async partial void OnShowInactiveStudentsChanged(bool value)
        {
            Filters.IsActive = !value ? true : null;
        }

        [RelayCommand]
        private async Task ResetFilters()
        {
            Filters = new();
            await RefreshOnFilterCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task RefreshOnFilterAsync()
        {
            CurrentPage = 1;
            await LoadStudentsCommand.ExecuteAsync(null);
        }

        private async void OnFiltersUpdated(object? sender, PropertyChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                await Task.Delay(300, token);
                await RefreshOnFilterCommand.ExecuteAsync(null);
            }
            catch (TaskCanceledException)
            {
            }
        }

        public async Task LoadAsync()
        {
            IsAdmin = _currentUser.IsAdmin();

            await CheckPermissionsCommand.ExecuteAsync(null);

            if (!CanViewStudents)
            {
                _messageService.Show("You do not have permission to view students");
                return;
            }

            await LoadStudentsCommand.ExecuteAsync(null);
        }
    }
}

