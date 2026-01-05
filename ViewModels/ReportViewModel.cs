using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using New_Student_Management.Helpers;
using New_Student_Management.Models;
using New_Student_Management.Services;
using System.Diagnostics;
using System.Windows;
using New_Student_Management.Reports;

namespace New_Student_Management.ViewModels
{
    public partial class ReportViewModel : ObservableObject, IAsyncLoadable
    {
        private readonly IStudentRepository _studentRepository;

        [ObservableProperty]
        private List<DataGridTabConstructor<Student>> _studentTabs;
        [ObservableProperty]
        private int _currentTabIndex;
        [ObservableProperty]
        private string _currentTabHeader;
        [ObservableProperty]
        private string _currentTabStudentCount;
        [ObservableProperty]
        private string _currentTabFemaleStudentCount;
        [ObservableProperty]
        private List<Student> _allStudents;
        [ObservableProperty]
        private List<Student> _computerStudents;
        [ObservableProperty]
        private List<Student> _electricalStudents;
        [ObservableProperty]
        private List<Student> _cNCStudents;
        [ObservableProperty]
        private Student? _selectedStudent;
        [ObservableProperty]
        private DateTime _reportDate = DateTime.Now;
        [ObservableProperty]
        private string _studyYear = "2025-2026";

        public ReportViewModel(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;

            // Initialize Students List
            AllStudents = [];
            ComputerStudents = [];
            ElectricalStudents = [];
            CNCStudents = [];

            // Default Tab
            StudentTabs =
            [
                new()
                {
                    Header = "គ្រប់ជំនាញ",
                    Data = AllStudents
                },
                new()
                {
                    Header = "កុំព្យូទ័រ",
                    Data = ComputerStudents
                },
                new()
                {
                    Header = "អគ្គិសនី",
                    Data = ElectricalStudents
                },
                new()
                {
                    Header = "មេកានិច",
                    Data = CNCStudents
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
            List<Student> students = AllStudents;
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
            List<Student> students = AllStudents;
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

        private async Task LoadStudentsAsync()
        {
            List<Student> students = await _studentRepository.GetAllStudentsAsync();
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
                }
            }

            OnCurrentTabIndexChanged(CurrentTabIndex);
        }

        partial void OnCurrentTabIndexChanged(int value)
        {
            if (StudentTabs == null || StudentTabs.Count <= value)
                return;

            var data = StudentTabs[value].Data;
            if (data == null)
                return;

            CurrentTabHeader = StudentTabs[value].Header;
            CurrentTabStudentCount = StudentTabs[value].Data.Count().UseKhmerNumbers();
            CurrentTabFemaleStudentCount = StudentTabs[value].Data.Count(s => s.Gender == StudentGender.Female).UseKhmerNumbers();
        }

        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }
    }
}