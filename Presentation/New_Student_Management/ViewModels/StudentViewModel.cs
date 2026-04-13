using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using New_Student_Management.Views.Wizards.Services;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace New_Student_Management.ViewModels
{
    public partial class StudentViewModel : ObservableObject, IAsyncLoadable
    {
        private readonly ICandidateService _candidateService;
        private readonly IPhotoDeleteService _photoDeleteService;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IMessageService _messageService;
        private readonly IEditStudentWizardService _editStudentWizardService;

        // Debouncing and async filtering
        private CancellationTokenSource? _filterCancellationTokenSource;
        private Timer? _searchDebounceTimer;
        private const int SearchDebounceDelayMs = 300;

        public StudentViewModel(
            ICandidateService candidateService,
            IPhotoDeleteService photoDeleteService,
            IPhotoFetchService photoFetchService,
            IMessageService messageService,
            IEditStudentWizardService editStudentWizardService)
        {
            _candidateService = candidateService;
            _photoDeleteService = photoDeleteService;
            _photoFetchService = photoFetchService;
            _messageService = messageService;
            _editStudentWizardService = editStudentWizardService;

            ToggleLatinNames = false;
        }

        private readonly int _studentsPerPage = 100;

        // Loading states
        [ObservableProperty]
        private bool _dataLoading = false;

        [ObservableProperty]
        private int? _lastId = null;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _maximumPages = 1;

        // ទិន្នន័យសិស្សទាំងអស់ដែលមាននៅក្នុង database
        public ObservableCollection<Candidate> AllStudents = [];

        // ទិន្នន័យសិស្សដែលត្រូវបានបង្ហាញនៅលើ UI
        [ObservableProperty]
        private ICollectionView? _studentsView;

        // សិស្សដែលកំពុងជ្រើសរើស
        [ObservableProperty]
        private Candidate? _selectedStudent;

        // ច្រោះទិន្នន័យតាម​ស្ថានភាពទិន្នន័យ
        [ObservableProperty]
        private StudentDataStateFilterOptions _dataStateFilter = StudentDataStateFilterOptions.All;

        partial void OnDataStateFilterChanged(StudentDataStateFilterOptions value)
        {
            // Only refresh the view; preserve SelectedStudent if it still matches the filter
            RefreshStudentsViewAsync();
        }

        // ទិន្នន័យសម្រាប់ Combo box ​ច្រោះទិន្នន័យតាម​ស្ថានភាពទិន្នន័យ
        public IEnumerable<object> DataStateFilterItems
        { get; } = Enum.GetValues<StudentDataStateFilterOptions>()
            .Select(s => new { Value = s, Description = $"{s.GetDescription()}" });

        // ច្រោះទិន្នន័យតាម​ការស្វែងរក
        [ObservableProperty]
        private string _studentSearch = string.Empty;

        partial void OnStudentSearchChanged(string value)
        {
            // Debounce the search: wait 300ms after user stops typing before filtering
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(
                (_) => RefreshStudentsViewAsync(),
                null,
                SearchDebounceDelayMs,
                Timeout.Infinite);
        }

        // ច្រោះទិន្នន័យតាម​ការស្វែងរក
        [ObservableProperty]
        private StudentField _currentSearchField = StudentField.FullName;

        partial void OnCurrentSearchFieldChanged(StudentField value)
        {
            RefreshStudentsViewAsync();
        }

        // Fields ដែលត្រូវបានរំលងក្នុងការ Filter/Search
        private static readonly HashSet<StudentField> IgnoredFields =
        [
            StudentField.FirstName,
            StudentField.LastName,
            StudentField.LatinFirstName,
            StudentField.LatinLastName,
            StudentField.PhotoPath,
            StudentField.CreatedAt,
        ];

        // ទិន្នន័យសម្រាប់ Combo box ​ច្រោះទិន្នន័យតាម​ការស្វែងរក
        public IEnumerable<object> StudentFieldItems { get; } =
            Enum.GetValues<StudentField>()
                .Where(f =>
                {
                    if (IgnoredFields.Contains(f))
                    {
                        return false;
                    }
                    return true;
                })
                .Select(f =>
                {
                    return new
                    {
                        Value = f,
                        Description = EnumExtensions.GetDescription(f)
                    };
                });

        // ផ្លាស់ប្ដូរការបង្ហាញឈ្មោះជាអក្សរខ្មែរនិងឡាតាំង
        [ObservableProperty]
        private bool _toggleLatinNames;

        partial void OnToggleLatinNamesChanged(bool value)
        {
            if (CurrentSearchField == StudentField.FullName || CurrentSearchField == StudentField.LatinFullName)
            {
                StudentField searchField = value
                    ? StudentField.LatinFullName
                    : StudentField.FullName;
                CurrentSearchField = searchField;
            }
            RefreshStudentsViewAsync();
        }

        // Relay Commands
        [RelayCommand]
        private async Task LoadStudentsAsync()
        {
            DataLoading = true;
            try
            {
                ReturnResponse<int> studentsCount = await _candidateService.GetAllCandidatesCountAsync();

                ReturnResponse<List<Candidate>> studentsResponse = await _candidateService.GetAllCandidatesAsync(LastId, _studentsPerPage);
                List<Candidate>? students = studentsResponse.Value;

                if (studentsCount.Status == ReturnStatus.Failed)
                {
                    _messageService.Show(studentsCount.Message, "Error", button: MessageButton.OK, MessageIcon.Error);
                    return;
                }

                if (studentsResponse.Status == ReturnStatus.Failed || students == null)
                {
                    _messageService.Show(studentsResponse.Message, "Error", button: MessageButton.OK, MessageIcon.Error);
                    return;
                }

                // Load data with minimal UI disruption
                AllStudents = [..students];
                MaximumPages = studentsCount.Value / _studentsPerPage;

                // Create collection view with optimized filtering
                if (StudentsView == null)
                {
                    StudentsView = CollectionViewSource.GetDefaultView(AllStudents);
                    StudentsView.Filter = FilterStudent;
                }
                else
                {
                    // Refresh the existing view to apply filters
                    StudentsView.Refresh();
                }
            }
            finally
            {
                DataLoading = false;
            }
        }

        [RelayCommand]
        private async Task EditStudentAsync()
        {
            if (SelectedStudent == null) return;
            Candidate previous = SelectedStudent;

            bool? result = _editStudentWizardService.Show(SelectedStudent);
            if (result == true)
            {
                await LoadStudentsAsync();
                SelectedStudent = previous;
            }
        }

        [RelayCommand]
        private async Task DeleteStudentAsync()
        {
            Candidate? student = SelectedStudent;

            if (student == null) return;

            MessageResult result = _messageService.Show($"តើអ្នកប្រាកដថាលុបសិស្សឈ្មោះ {student.FullName} ដែរឬទេ?", "Confirm Delete", MessageButton.YesNo, MessageIcon.Exclamation);
            if (result != MessageResult.Yes) return;

            ReturnResponse deleteResponse = await _candidateService.DeleteCandidateAsync(student.Id);

            if (deleteResponse.Status == ReturnStatus.Failed || deleteResponse.Status == ReturnStatus.Rejected)
            {
                _messageService.Show($"{deleteResponse.Message}", "ហាក... ស្អីគេ?", icon: MessageIcon.Error);
                return;
            }

            ReturnResponse deletePhotoResponse = await _photoDeleteService.DeleteStudentPhoto(student.PhotoKey);

            if (deletePhotoResponse.Status == ReturnStatus.Failed || deletePhotoResponse.Status == ReturnStatus.Rejected)
            {
                _messageService.Show($"{deletePhotoResponse.Message}", "ហាក... ស្អីគេ?", icon: MessageIcon.Error);
                return;
            }

            await LoadStudentsAsync();
        }

        // Initial async load
        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }

        // Helper functions
        /// <summary>
        /// Refreshes the students view with optimized filtering.
        /// Uses async cancellation to avoid processing stale filter requests.
        /// </summary>
        private void RefreshStudentsViewAsync()
        {
            if (StudentsView == null) return;

            // Cancel any previous filter operation
            _filterCancellationTokenSource?.Cancel();
            _filterCancellationTokenSource = new CancellationTokenSource();

            // Refresh on UI thread for data binding consistency
            StudentsView.Refresh();
        }
        private bool CheckSelectedStudent()
        {
            if (SelectedStudent != null) return true;
            else return false;
        }

        private static bool CandidateValidation(Candidate student)
        {
            // Check for null early to avoid unnecessary validation calls
            if (student == null) return false;

            ValidationResponse response = student.HasAllData("Age", "OtherInfo", "CreatedAt", "LatinFullName", "FullName");
            return response.IsValid;
        }

        private bool FilterStudent(object obj)
        {
            if (obj is not Candidate student) return false;

            // SEARCH FILTER - Early exit if search doesn't match
            if (!string.IsNullOrWhiteSpace(StudentSearch))
            {
                string keyword = StudentSearch.Trim().ToLower();
                bool match = StudentFilters.MatchSearch(student, keyword, CurrentSearchField);

                if (!match)
                    return false;
            }

            // DATA STATE FILTER - Use switch expression for efficient filtering
            // Validate candidate only once, then use result for all comparisons
            bool isValidated = CandidateValidation(student);
            bool hasPhoto = !string.IsNullOrEmpty(student.PhotoKey);

            return DataStateFilter switch
            {
                StudentDataStateFilterOptions.Completed => isValidated && hasPhoto,
                StudentDataStateFilterOptions.MissingData => !isValidated && hasPhoto,
                StudentDataStateFilterOptions.NoPicture => !hasPhoto,
                StudentDataStateFilterOptions.MissingDataAndPicture => !isValidated || !hasPhoto,
                _ => true,
            };
        }
    }
}