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
using System.Windows;
using System.Windows.Data;

namespace New_Student_Management.ViewModels
{
    public partial class ReportViewModel : ObservableObject, IAsyncLoadable
    {
        private readonly IStudentRepository _studentRepository;

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
        private List<Candidate> _computerStudents;
        [ObservableProperty]
        private List<Candidate> _electricalStudents;
        [ObservableProperty]
        private List<Candidate> _cNCStudents;
        [ObservableProperty]
        private List<Candidate> _generalStudents;
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
        public ICollectionView ComputerStudentsView { get; }
        public ICollectionView ElectricalStudentsView { get; }
        public ICollectionView CNCStudentsView { get; }
        public ICollectionView GeneralStudentsView { get; }

        public ReportViewModel(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;

            // Initialize Students List
            AllStudents = [];
            ComputerStudents = [];
            ElectricalStudents = [];
            CNCStudents = [];
            GeneralStudents = [];

            AllStudentsView = CollectionViewSource.GetDefaultView(AllStudents);
            ComputerStudentsView = CollectionViewSource.GetDefaultView(ComputerStudents);
            ElectricalStudentsView = CollectionViewSource.GetDefaultView(ElectricalStudents);
            CNCStudentsView = CollectionViewSource.GetDefaultView(CNCStudents);
            GeneralStudentsView = CollectionViewSource.GetDefaultView(GeneralStudents);

            AllStudentsView.Filter = StudentFilter;
            ComputerStudentsView.Filter = StudentFilter;
            ElectricalStudentsView.Filter = StudentFilter;
            CNCStudentsView.Filter = StudentFilter;
            GeneralStudentsView.Filter = StudentFilter;

            // Default Tab
            StudentTabs =
            [
                new()
                {
                    Header = "គ្រប់ជំនាញ",
                    Data = AllStudentsView,
                },
                new()
                {
                    Header = "កុំព្យូទ័រ",
                    Data = ComputerStudentsView
                },
                new()
                {
                    Header = "អគ្គិសនី",
                    Data = ElectricalStudentsView
                },
                new()
                {
                    Header = "មេកានិច",
                    Data = CNCStudentsView
                },
                new() {
                    Header = "ចំណេះទូទៅ",
                    Data = GeneralStudentsView
                }
            ];

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

            DepartmentReport departmentReport = new(students, int.Parse(StudyYear.Split('-')[0]), int.Parse(StudyYear.Split('-')[1]));

            ReturnStatus status = await Task.Run(() => departmentReport.GenerateReport());

            if (status == ReturnStatus.Success)
            {
                MessageBoxResult result = MessageBox.Show("Report generated successfully!\n Do you wish to see the files?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result.Equals(MessageBoxResult.Yes))
                {
                    Process.Start("explorer.exe", $"{departmentReport.OutputPath}\\{departmentReport.FileName}");
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

            CandidateReport candidateReport = new(students, ReportDate, int.Parse(StudyYear.Split('-')[0]), int.Parse(StudyYear.Split('-')[1]));

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

            if (s.HasAllData() == false && HideStudents == true)
            {
                return false;
            }

            return true;
        }

        private void RefreshFilters()
        {
            AllStudentsView.Refresh();
            ComputerStudentsView.Refresh();
            ElectricalStudentsView.Refresh();
            CNCStudentsView.Refresh();
            GeneralStudentsView.Refresh();
        }
        private void UpdateCurrentTabStats(int value)
        {
            var view = StudentTabs[value].Data;

            CurrentTabStudentCount =
                view.Cast<Candidate>().Count()
                    .UseKhmerNumbers();

            CurrentTabFemaleStudentCount =
                view.Cast<Candidate>().Count()
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
            if (StudentTabs == null || StudentTabs.Count <= value)
                return;

            ICollectionView? data = StudentTabs[value].Data;
            if (data == null)
                return;

            CurrentTabHeader = StudentTabs[value].Header;
            UpdateCurrentTabStats(value);
        }

        private async Task LoadStudentsAsync()
        {
            List<Candidate> students = await _studentRepository.GetAllStudentsAsync();
            AllStudents.Clear();
            ComputerStudents.Clear();
            ElectricalStudents.Clear();
            CNCStudents.Clear();

            foreach (var s in students)
            {
                AllStudents.Add(s);

                switch (s.Skill)
                {
                    case StudentSkill.Computer:
                        ComputerStudents.Add(s);
                        break;
                    case StudentSkill.Electrical:
                        ElectricalStudents.Add(s);
                        break;
                    case StudentSkill.CNC:
                        CNCStudents.Add(s);
                        break;
                    case StudentSkill.General:
                        GeneralStudents.Add(s);
                        break;
                }

                AllStudentsView.Refresh();
                ComputerStudentsView.Refresh();
                ElectricalStudentsView.Refresh();
                CNCStudentsView.Refresh();
                GeneralStudentsView.Refresh();
            }

            OnCurrentTabIndexChanged(CurrentTabIndex);
        }
        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }
    }
}