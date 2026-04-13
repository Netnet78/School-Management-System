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

namespace New_Student_Management.ViewModels
{
    public partial class ReportViewModel : ObservableObject, IAsyncLoadable
    {
        private readonly ICandidateRepository _studentRepository;
        private readonly ISkillRepository _skillRepository;

        [ObservableProperty]
        private List<DataGridTabConstructor> _studentTabs;

        [ObservableProperty]
        private int _currentTabIndex;

        [ObservableProperty]
        private string _currentTabHeader;

        [ObservableProperty]
        private string _currentTabStudentCount;

        [ObservableProperty]
        private string _currentTabFemaleStudentCount;

        [ObservableProperty]
        private List<Candidate> _allStudents;

        [ObservableProperty]
        private Candidate? _selectedStudent;

        [ObservableProperty]
        private DateTime _reportDate = DateTime.Now;

        [ObservableProperty]
        private string _studyYear = "2025-2026";

        [ObservableProperty]
        private DateTime? createdAtStart = null;

        [ObservableProperty]
        private DateTime? createdAtEnd = null;

        [ObservableProperty]
        private bool hideStudents = true; // Property for hiding students that don't have enough information

        public ICollectionView AllStudentsView { get; }

        public ReportViewModel(ICandidateRepository studentRepository, ISkillRepository skillRepository)
        {
            _studentRepository = studentRepository;
            _skillRepository = skillRepository;

            // Initialize Students List
            AllStudents = [];

            AllStudentsView = CollectionViewSource.GetDefaultView(AllStudents);

            // Default Tab
            StudentTabs = [];

            CurrentTabStudentCount = "០";
            CurrentTabFemaleStudentCount = "០";
            CurrentTabHeader = "";
            CurrentTabIndex = 0;
        }

        [RelayCommand]
        private async Task GenerateDepartmentReportAsync()
        {
            List<Candidate> students = [];

            foreach (Candidate student in AllStudentsView)
            {
                students.Add(student);
            }

            List<Skill> skills = await _skillRepository.GetAllAsync();

            DepartmentReport departmentReport = new(int.Parse(StudyYear.Split('-')[0]), int.Parse(StudyYear.Split('-')[1]), skills);

            ReturnResponse response = await Task.Run(() => departmentReport.GenerateReport());

            if (response.Status == ReturnStatus.Success)
            {
                MessageBoxResult result = MessageBox.Show("Report generated successfully!\n Do you wish to see the files?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result.Equals(MessageBoxResult.Yes))
                {
                    Process.Start("explorer.exe", Path.Combine(departmentReport.OutputPath, departmentReport.FileName));
                }
            }
        }

        [RelayCommand]
        private async Task GenerateCandidateReportAsync()
        {
            List<Candidate> students = [];

            foreach (Candidate student in AllStudentsView)
            {
                students.Add(student);
            }

            List<Skill> skills = await _skillRepository.GetAllAsync();

            CandidateReport candidateReport = new(ReportDate, int.Parse(StudyYear.Split('-')[0]), int.Parse(StudyYear.Split('-')[1]), skills);

            ReturnStatus status = await Task.Run(() => candidateReport.GenerateReport());

            if (status == ReturnStatus.Success)
            {
                MessageBoxResult result = MessageBox.Show("Report generated successfully!\n Do you wish to see the files?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result.Equals(MessageBoxResult.Yes))
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

            if (CurrentTabIndex >= 0 && CurrentTabIndex < StudentTabs?.Count)
            {
                StudentTabs[CurrentTabIndex].Data?.Refresh();
            }
        }

        private void UpdateCurrentTabStats(int value)
        {
            if (value < 0 || StudentTabs == null || value >= StudentTabs.Count)
                return;

            // Offload to thread pool to avoid blocking UI
            _ = Task.Run(() =>
            {
                var view = StudentTabs[value].Data;
                if (view == null) return;

                var candidates = view.Cast<Candidate>().ToList();

                var totalCount = candidates.Count.UseKhmerNumbers();
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

            if (StudentTabs == null || StudentTabs.Count <= value)
                return;

            ICollectionView? data = StudentTabs[value].Data;
            if (data == null)
                return;

            CurrentTabHeader = StudentTabs[value].Header;
            UpdateCurrentTabStats(value);
        }

        [RelayCommand]
        public async Task LoadStudentsAsync()
        {
            List<Candidate> students = await _studentRepository.GetCandidatesOnlyAsync(null, int.MaxValue);

            // Move heavy grouping to background thread, but NOT CollectionViewSource creation
            var groupedData = await Task.Run(() =>
            {
                var grouped = students.GroupBy(s => s.Skill)
                    .Where(g => g.Key != null && g.Count() > 0)
                    .Select(g => new
                    {
                        g.Key,
                        Items = g.ToList()
                    })
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[GroupingDebug] Total students: {students.Count}");
                System.Diagnostics.Debug.WriteLine($"[GroupingDebug] Grouped into {grouped.Count} skill groups");
                foreach (var g in grouped)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {g.Key.Name}: {g.Items.Count} students");
                }

                return grouped;
            });

            AllStudents.Clear();
            foreach (var s in students)
            {
                AllStudents.Add(s);
            }

            // Create CollectionViewSource on UI thread
            var tabs = new List<DataGridTabConstructor>
            {
                new DataGridTabConstructor
                {
                    Header = "គ្រប់ជំនាញ",
                    Data = CollectionViewSource.GetDefaultView(AllStudents)
                }
            };

            System.Diagnostics.Debug.WriteLine($"[TabCreation] Creating tabs. GroupedData count: {groupedData.Count}");

            // Add grouped tabs on UI thread
            foreach (var group in groupedData)
            {
                var list = group.Items;
                var view = CollectionViewSource.GetDefaultView(list);
                view.Filter = StudentFilter;

                tabs.Add(new DataGridTabConstructor
                {
                    Header = group.Key.Name,
                    Data = view,
                });

                System.Diagnostics.Debug.WriteLine($"[TabCreation] Added tab: {group.Key.Name} with {list.Count} students");
            }

            StudentTabs = tabs;

            System.Diagnostics.Debug.WriteLine($"[TabCreation] Total tabs created: {StudentTabs.Count}");

            // Set filter on main view
            AllStudentsView.Filter = StudentFilter;
            AllStudentsView.Refresh();
            OnCurrentTabIndexChanged(CurrentTabIndex);
        }

        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }
    }
}