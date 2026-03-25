using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using New_Student_Management.Reports;
using School_Management.Application.Services;
using School_Management.Core.Enums;
using School_Management.Core.Models;
using School_Management.Infrastructure.Repositories;
using School_Management.Presentation.Shared.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;

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

            ReturnStatus status = await Task.Run(() => departmentReport.GenerateReport());

            if (status == ReturnStatus.Success)
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

            CandidateReport candidateReport = new(students, ReportDate, int.Parse(StudyYear.Split('-')[0]), int.Parse(StudyYear.Split('-')[1]), skills);

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

            if (CreatedAtStart.HasValue &&  s.CreatedAt.Date < CreatedAtStart.Value.Date)
            {
                return false;
            }
            if (CreatedAtEnd.HasValue && s.CreatedAt.Date > CreatedAtEnd.Value.Date)
            {
                return false;
            }

            if (s.HasAllData("Age", "OtherInfo", "CreatedAt", "LatinFullName", "FullName") == false && HideStudents == true)
            {
                return false;
            }

            return true;
        }

        private void RefreshFilters()
        {
            AllStudentsView.Refresh();
        }
        private void UpdateCurrentTabStats(int value)
        {
            var view = StudentTabs[value].Data;

            CurrentTabStudentCount =
                view.Cast<Candidate>().Count()
                    .UseKhmerNumbers();

            CurrentTabFemaleStudentCount =
                view.Cast<Candidate>()
                    .Count(c => c.Gender == Gender.Female)
                    .UseKhmerNumbers();
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
            List<Candidate> students = await _studentRepository.GetCandidatesOnlyAsync();
            AllStudents.Clear();

            foreach (var s in students)
            {
                AllStudents.Add(s);
            }

            List<DataGridTabConstructor> tabs =
            [
                new DataGridTabConstructor
                {
                    Header = "គ្រប់ជំនាញ",
                    Data = CollectionViewSource.GetDefaultView(AllStudents)
                },
            ];

            var groupedStudents = students.GroupBy(s => s.Skill);

            foreach (var group in groupedStudents)
            {
                if (group.Key.Students.Count <= 0)
                {
                    continue;
                }

                List<Candidate> list = group.ToList();
                ICollectionView view = CollectionViewSource.GetDefaultView(list);
                view.Filter = StudentFilter;

                tabs.Add(new DataGridTabConstructor
                {
                    Header = group.Key.Name,
                    Data = view,
                });
            }

            StudentTabs = tabs;

            AllStudentsView.Refresh();
            OnCurrentTabIndexChanged(CurrentTabIndex);
        }
        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }
    }
}