using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Application.Services;
using School_Management.Core.Enums;
using School_Management.Core.Models;
using School_Management.Infrastructure.Repositories;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace New_Student_Management.ViewModels
{
    public enum DataStateFilterOptions
    {
        [Description("គ្រប់ប្រភេទ")]
        All,
        [Description("គ្រប់គ្រាន់")]
        Completed,
        [Description("ខ្វះខាតទិន្នន័យ")]
        MissingData,
        [Description("ខ្វះខាតរូបភាព")]
        NoPicture,
        [Description("ខ្វះខាតទិន្នន័យ និងរូបភាព")]
        MissingDataAndPicture,
    }

    public partial class StudentViewModel : ObservableObject, IAsyncLoadable
    {
        private readonly IStudentRepository _repository;
        public StudentViewModel(IStudentRepository repository)
        {
            _repository = repository;
            ToggleLatinNames = false;
        }

        // Loading states
        [ObservableProperty]
        private bool _dataLoading = false;

        // MVVM Bindings
        [ObservableProperty]
        private List<Candidate> _allStudents = [];
        private ICollectionView? _studentsView;
        public ICollectionView? Students
        {
            get => _studentsView;
            private set => SetProperty(ref _studentsView, value);
        }

        // សិស្សដែលកំពុងជ្រើសរើស
        private Candidate? _selectedStudent;
        // សិស្សដែលកំពុងជ្រើសរើស
        public Candidate? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                SetProperty(ref _selectedStudent, value);
            }
        }
        // ច្រោះទិន្នន័យតាម​ស្ថានភាពទិន្នន័យ
        private DataStateFilterOptions _dataStateFilter = DataStateFilterOptions.All;
        // ច្រោះទិន្នន័យតាម​ស្ថានភាពទិន្នន័យ
        public DataStateFilterOptions DataStateFilter
        {
            get => _dataStateFilter;
            set
            {
                SetProperty(ref _dataStateFilter, value);
                Students?.Refresh();
            }
        }
        // ទិន្នន័យសម្រាប់ Combo box ​ច្រោះទិន្នន័យតាម​ស្ថានភាពទិន្នន័យ
        public IEnumerable<object> DataStateFilterItems
        { get; } = Enum.GetValues<DataStateFilterOptions>()
            .Select(s => new { Value = s, Description = $"{EnumExtensions.GetDescription(s)}"});
        // ច្រោះទិន្នន័យតាម​ការស្វែងរក
        private string _studentSearch = string.Empty;
        // ច្រោះទិន្នន័យតាម​ការស្វែងរក
        public string StudentSearch
        {
            get => _studentSearch;
            set
            {
                SetProperty(ref _studentSearch, value);
                Students?.Refresh();
            }
        }
        // ច្រោះទិន្នន័យតាម​ការស្វែងរក
        private StudentField _currentSearchField = StudentField.FullName;
        public StudentField CurrentSearchField
        {
            get => _currentSearchField;
            set
            {
                SetProperty(ref _currentSearchField, value);
                StudentSearch = string.Empty;
                Students?.Refresh();
            }
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
        private bool _toggleLatinNames;
        // ផ្លាស់ប្ដូរការបង្ហាញឈ្មោះជាអក្សរខ្មែរនិងឡាតាំង
        public bool ToggleLatinNames
        {
            get => _toggleLatinNames;
            set
            {
                SetProperty(ref _toggleLatinNames, value);

                if (CurrentSearchField == StudentField.FullName || CurrentSearchField == StudentField.LatinFullName)
                {
                    StudentField searchField = value
                        ? StudentField.LatinFullName
                        : StudentField.FullName;
                    CurrentSearchField = searchField;
                }
                Students?.Refresh();
            }
        }

        // Relay Commands
        [RelayCommand]
        private async Task LoadStudentsAsync()
        {
            try
            {
                DataLoading = true;
                List<Candidate> students = await _repository.GetAllStudentsAsync();

                AllStudents = [.. students];
                Students = CollectionViewSource.GetDefaultView(AllStudents);
                Students.Filter = FilterStudent;

                SelectedStudent = null;
            }
            finally
            {
                DataLoading = false;
            }
        }
        [RelayCommand]
        private async Task EditStudentAsync(Candidate student)
        {
            if (SelectedStudent == null) return;

            Candidate previous = SelectedStudent;
            var vm = new EditStudentViewModel(SelectedStudent, _repository);
            var wizard = new Views.Wizards.EditStudentWizard(vm)
            {
                Owner = App.Current.MainWindow,
            };
            bool? result = wizard.ShowDialog();
            if (result == true)
            {
                await LoadStudentsAsync();
                SelectedStudent = previous;
            }
        }
        [RelayCommand]
        private async Task DeleteStudentAsync(Candidate student)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {student.FullName}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            if (SelectedStudent == null) return;
            await _repository.DeleteStudentAsync(student);
            await LoadStudentsAsync();
        }
        [RelayCommand]
        private async Task SaveStudentAsync()
        {
            await _repository.SaveStudentAsync();
        }

        // Initial async load
        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }

        // Helper functions
        private bool FilterStudent(object obj)
        {
            if (obj is not Candidate student) return false;

            // 🔎 SEARCH FILTER
            if (!string.IsNullOrWhiteSpace(StudentSearch))
            {
                string keyword = StudentSearch.Trim().ToLower();

                bool match = StudentFilters.MatchSearch(student, keyword, CurrentSearchField);

                if (!match)
                    return false;
            }

            // 📊 DATA STATE FILTER
            return DataStateFilter switch
            {
                DataStateFilterOptions.Completed => student.HasAllData(),
                DataStateFilterOptions.MissingData => !student.HasAllData() && !string.IsNullOrEmpty(student.PhotoPath),
                DataStateFilterOptions.NoPicture => string.IsNullOrEmpty(student.PhotoPath),
                DataStateFilterOptions.MissingDataAndPicture => string.IsNullOrEmpty(student.PhotoPath) || !student.HasAllData(),
                _ => true,
            };
        }

    }
}
