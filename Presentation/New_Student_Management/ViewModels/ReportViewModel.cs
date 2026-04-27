using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using New_Student_Management.Reports;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Models;
using School_Management.Presentation.Shared.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using KhmerCalendar;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Interfaces;
using School_Management.Core.Interfaces.Application;

namespace New_Student_Management.ViewModels
{
    public partial class ReportViewModel : ObservableObject, IAsyncLoadable, IViewModel
    {
        private readonly ICandidateRepository _studentRepository;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IMessageService _messageService;

        [ObservableProperty]
        private IEnumerable<DataGridTabConstructor> _studentTabs;

        [ObservableProperty]
        private int _currentTabIndex;

        [ObservableProperty]
        private string _currentTabHeader;

        [ObservableProperty]
        private string _currentTabStudentCount;

        [ObservableProperty]
        private string _currentTabFemaleStudentCount;

        [ObservableProperty]
        private IEnumerable<Candidate> _allStudents;

        [ObservableProperty]
        private Candidate? _selectedStudent;

        [ObservableProperty]
        private DateTime _reportDate = DateTime.Now;

        [ObservableProperty]
        private DateTime? createdAtStart = new DateTime(DateTime.Now.Year, 1, 1);

        [ObservableProperty]
        private DateTime? createdAtEnd = new DateTime(DateTime.Now.Year + 1, 12, 
            DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

        [ObservableProperty]
        private string _studyYear = string.Empty;

        [ObservableProperty]
        private bool hideStudents = true; // Property for hiding students that don't have enough information

        public ICollectionView AllStudentsView { get; private set; }

        public ReportViewModel(ICandidateRepository studentRepository, IMessageService messageService, IPhotoFetchService photoFetchService)
        {
            _studentRepository = studentRepository;
            _messageService = messageService;
            _photoFetchService = photoFetchService;

            // Initialize Students List
            AllStudents = [];

            AllStudentsView = CollectionViewSource.GetDefaultView(AllStudents);

            // Default Tab
            StudentTabs = [];

            CurrentTabStudentCount = "០";
            CurrentTabFemaleStudentCount = "០";
            CurrentTabHeader = "";
            CurrentTabIndex = 0;

            if (CreatedAtStart != null)
            {
                DateTime value = CreatedAtStart.Value;
                StudyYear += value.Year + "-";
            }
            if (CreatedAtEnd != null)
            {
                DateTime value = CreatedAtEnd.Value;
                StudyYear += value.Year;
            }
        }

        [RelayCommand]
        private async Task GenerateDepartmentReportAsync()
        {
            List<Candidate> students = GetCurrentFilteredStudents();

            if (students.Count == 0)
            {
                _messageService.Show(
                    "No candidates match the current filters. Please adjust date range or disable hiding incomplete students, then try again.",
                    "No Data",
                    MessageButton.OK,
                    MessageIcon.Exclamation);
                return;
            }

            DepartmentReport departmentReport = new(int.Parse(StudyYear.Split('-')[0]), int.Parse(StudyYear.Split('-')[1]), students, _photoFetchService);

            ReturnResponse response = await Task.Run(() => departmentReport.GenerateReport());

            if (response.Status == ReturnStatus.Success)
            {
                MessageResult result = _messageService.Show("Report generated successfully!\n Do you wish to see the files?", "Success", MessageButton.YesNo, MessageIcon.Information);
                if (result.Equals(MessageResult.Yes))
                {
                    Process.Start("explorer.exe", Path.Combine(departmentReport.OutputPath, departmentReport.FileName));
                }
            }
        }

        [RelayCommand]
        private async Task GenerateCandidateReportAsync()
        {
            List<Candidate> students = GetCurrentFilteredStudents();

            if (students.Count == 0)
            {
                _messageService.Show(
                    "No candidates match the current filters. Please adjust date range or disable hiding incomplete students, then try again.",
                    "No Data",
                    MessageButton.OK,
                    MessageIcon.Exclamation);
                return;
            }

            CandidateReport candidateReport = new(ReportDate, int.Parse(StudyYear.Split('-')[0]), int.Parse(StudyYear.Split('-')[1]), students);

            ReturnStatus status = await Task.Run(() => candidateReport.GenerateReport());

            if (status == ReturnStatus.Success)
            {
                MessageResult result = _messageService.Show("Report generated successfully!\n Do you wish to see the files?", "Success", MessageButton.YesNo, MessageIcon.Information);
                if (result.Equals(MessageResult.Yes))
                {
                    Process.Start("explorer.exe", $"{candidateReport.OutputPath}\\{candidateReport.FileName}");
                }
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            CreatedAtStart = null;
            CreatedAtEnd = null;
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            await LoadStudentsAsync();
        }

        private bool StudentFilter(object obj)
        {
            if (obj is not Candidate s)
                return false;

            // Short-circuit early for better performance
            if (CreatedAtStart.HasValue && s.CreatedAt.Date < CreatedAtStart.Value.Date)
                return false;

            if (CreatedAtEnd.HasValue && s.CreatedAt.Date > CreatedAtEnd.Value.Date)
                return false;

            // Only check data validity if hiding incomplete students
            if (HideStudents && s.HasAllData("Age", "OtherInfo", "CreatedAt", "LatinFullName", "FullName").IsValid == false)
                return false;

            return true;
        }

        private void RefreshFilters()
        {
            AllStudentsView?.Refresh();

            if (CurrentTabIndex >= 0 && CurrentTabIndex < StudentTabs?.Count())
            {
                StudentTabs.ElementAt(CurrentTabIndex).Data?.Refresh();
            }
        }

        private List<Candidate> GetCurrentFilteredStudents()
        {
            ICollectionView? activeView = null;

            if (StudentTabs != null && CurrentTabIndex >= 0 && CurrentTabIndex < StudentTabs.Count())
            {
                activeView = StudentTabs.ElementAt(CurrentTabIndex).Data;
            }

            activeView ??= AllStudentsView;

            return activeView?.Cast<Candidate>().ToList() ?? [];
        }

        private void UpdateCurrentTabStats(int value)
        {
            if (value < 0 || StudentTabs == null || value >= StudentTabs.Count())
                return;

            // Offload to thread pool to avoid blocking UI
            _ = Task.Run(() =>
            {
                var view = StudentTabs.ElementAt(value).Data;
                if (view == null) return;

                var candidates = view.Cast<Candidate>().ToList();

                var totalCount = candidates.Count().UseKhmerNumbers();
                var femaleCount = candidates.Count(c => c.Gender == Gender.Female).UseKhmerNumbers();

                // Update UI on main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentTabStudentCount = totalCount;
                    CurrentTabFemaleStudentCount = femaleCount;
                });
            });
        }

        partial void OnCreatedAtStartChanged(DateTime? value) => RefreshFilters();

        partial void OnCreatedAtEndChanged(DateTime? value) => RefreshFilters();

        partial void OnHideStudentsChanged(bool value)
        {
            RefreshFilters();
            UpdateCurrentTabStats(CurrentTabIndex);
        }

        partial void OnCurrentTabIndexChanged(int value)
        {
            if (value < 0)
                return;

            if (StudentTabs == null || StudentTabs.Count() <= value)
                return;

            ICollectionView? data = StudentTabs.ElementAt(value).Data;
            if (data == null)
                return;

            CurrentTabHeader = StudentTabs.ElementAt(value).Header;
            RefreshFilters();
            UpdateCurrentTabStats(value);
        }

        [RelayCommand]
        public async Task LoadStudentsAsync()
        {
            IEnumerable<Candidate> students = await _studentRepository.GetCandidatesOnlyPagedAsync(1, 100);

            // Move heavy grouping to background thread, but NOT CollectionViewSource creation
            var groupedData = students
                .Where(g => g.Skill != null && g.Skill.IsActive)
                .GroupBy(s => s.Skill.Id)
                .Select(g => new
                {
                    g.First().Skill,
                    Items = g.ToList()
                })
                .ToList();

            AllStudents = [..students];

            ICollectionView mainView = CollectionViewSource.GetDefaultView(AllStudents);
            mainView.Filter = StudentFilter;
            AllStudentsView = mainView;

            var tabs = new List<DataGridTabConstructor>
            {
                new DataGridTabConstructor
                {
                    Header = "គ្រប់ជំនាញ",
                    Data = mainView
                }
            };

            Debug.WriteLine($"[TabCreation] Creating tabs. GroupedData count: {groupedData.Count()}");

            // Add grouped tabs on UI thread
            foreach (var group in groupedData)
            {
                var list = group.Items;
                var view = CollectionViewSource.GetDefaultView(list);
                view.Filter = StudentFilter;

                tabs.Add(new DataGridTabConstructor
                {
                    Header = group.Skill.KhmerName,
                    Data = view,
                });

                Debug.WriteLine($"[TabCreation] Added tab: {group.Skill.Name} with {list.Count()} students");
            }

            StudentTabs = tabs;

            Debug.WriteLine($"[TabCreation] Total tabs created: {StudentTabs.Count()}");

            // Set filter on main view
            AllStudentsView.Refresh();
            OnCurrentTabIndexChanged(CurrentTabIndex);
        }

        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }
    }
}
