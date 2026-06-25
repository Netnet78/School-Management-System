using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolManagement.Application;
using SchoolManagement.Infrastructure;
using SchoolManagement.Presentation.Features.Attendances.ViewModels;
using SchoolManagement.Presentation.Features.Attendances.Views;
using SchoolManagement.Presentation.Features.AuditLogs.ViewModels;
using SchoolManagement.Presentation.Features.AuditLogs.Views;
using SchoolManagement.Presentation.Features.Classes.ViewModels;
using SchoolManagement.Presentation.Features.Classes.Views;
using SchoolManagement.Presentation.Features.Dashboard.ViewModels;
using SchoolManagement.Presentation.Features.Dashboard.Views;
using SchoolManagement.Presentation.Features.Departments.Views;
using SchoolManagement.Presentation.Features.Employees.ViewModels;
using SchoolManagement.Presentation.Features.Employees.Views;
using SchoolManagement.Presentation.Features.Roles.ViewModels;
using SchoolManagement.Presentation.Features.Roles.Views;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.ViewModels;
using SchoolManagement.Presentation.Features.Reports.Views;
using SchoolManagement.Presentation.Features.Reports.ViewProviders.Attendance;
using SchoolManagement.Presentation.Features.Reports.ViewProviders.Score;
using SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentCard;
using SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentRoster;
using SchoolManagement.Presentation.Features.Scores.ViewModels;
using SchoolManagement.Presentation.Features.Scores.Views;
using SchoolManagement.Presentation.Features.Subjects.ViewModels;
using SchoolManagement.Presentation.Features.Subjects.Views;
using SchoolManagement.Presentation.Features.Students.ViewModels;
using SchoolManagement.Presentation.Features.Students.Views;
using SchoolManagement.Presentation.Shell.ViewModels;
using SchoolManagement.Presentation.Shell.Views;
using System.Windows;
using SchoolManagement.Presentation.Features.Reports.Extensions;

namespace SchoolManagement.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static new App Current => (App)System.Windows.Application.Current;
        public IHost AppHost { get; }
        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddInfrastructure();
                    services.AddApplication();
                    services.AddPresentationShared();

                    // Register ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<AttendanceViewModel>();
                    services.AddTransient<AddAttendanceViewModel>();
                    services.AddTransient<EditAttendanceViewModel>();
                    services.AddTransient<SelectClassViewModel>();
                    services.AddTransient<SelectStudentViewModel>();
                    services.AddTransient<WizardAddAttendanceViewModel>();
                    services.AddTransient<ClassViewModel>();
                    services.AddTransient<StudentListViewModel>();
                    services.AddTransient<AddStudentViewModel>();
                    services.AddTransient<AssignStudentClassViewModel>();
                    services.AddTransient<ReportViewModel>();

                    // Auto-discover report types from [ReportType] attribute on generators
                    services.AddReportTypesFromAssembly(typeof(IReportGenerator).Assembly);

                    services.AddTransient<EmployeeViewModel>();
                    services.AddTransient<AddEmployeeViewModel>();
                    services.AddTransient<EditEmployeeViewModel>();
                    services.AddTransient<EmployeeUserViewModel>();
                    services.AddTransient<RoleManagementViewModel>();
                    services.AddTransient<AuditLogViewModel>();
                    services.AddTransient<EditStudentViewModel>();
                    services.AddTransient<AddClassViewModel>();
                    services.AddTransient<EditClassViewModel>();
                    services.AddTransient<ClassStudentListViewModel>();
                    services.AddTransient<DepartmentViewModel>();
                    services.AddTransient<ScoreViewModel>();
                    services.AddTransient<SubjectAssignmentViewModel>();
                    services.AddTransient<SubjectListViewModel>();
                    services.AddTransient<AddSubjectViewModel>();
                    services.AddTransient<EditSubjectViewModel>();
                    services.AddTransient<AddStudentOptionViewModel>();
                    services.AddTransient<AssignCandidateViewModel>();
                    services.AddTransient<StudentCardOptionsViewModel>();
                    services.AddTransient<AddStudentsToClassViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                    services.AddTransient<DashboardView>();
                    services.AddTransient<AttendanceView>();
                    services.AddTransient<AddAttendanceView>();
                    services.AddTransient<EditAttendanceView>();
                    services.AddTransient<SelectClassView>();
                    services.AddTransient<SelectStudentView>();
                    services.AddTransient<WizardAddAttendanceView>();
                    services.AddTransient<ClassView>();
                    services.AddTransient<StudentListView>();
                    services.AddTransient<AddStudentView>();
                    services.AddTransient<AssignStudentClassView>();
                    services.AddTransient<ReportView>();
                    services.AddTransient<StudentRosterFilterView>();
                    services.AddTransient<AttendanceFilterView>();
                    services.AddTransient<ScoreFilterView>();
                    services.AddTransient<StudentCardFilterView>();
                    services.AddTransient<StudentCardOptionsView>();
                    services.AddTransient<AddStudentsToClassView>();

                    // Report preview views
                    services.AddTransient<ReportTablePreviewView>();

                    services.AddTransient<EmployeeView>();
                    services.AddTransient<AddEmployeeView>();
                    services.AddTransient<EditEmployeeView>();
                    services.AddTransient<EmployeeUserView>();
                    services.AddTransient<RoleManagementView>();
                    services.AddTransient<AuditLogView>();
                    services.AddTransient<EditStudentView>();
                    services.AddTransient<AddClassView>();
                    services.AddTransient<EditClassView>();
                    services.AddTransient<ClassStudentListView>();
                    services.AddTransient<DepartmentView>();
                    services.AddTransient<ScoreView>();
                    services.AddTransient<SubjectAssignmentView>();
                    services.AddTransient<SubjectListView>();
                    services.AddTransient<AddSubjectView>();
                    services.AddTransient<EditSubjectView>();
                    services.AddTransient<AddStudentOptionView>();
                    services.AddTransient<AssignCandidateView>();
                }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            IServiceProvider serviceProvider = AppHost.Services;

            if (serviceProvider == null)
            {
                MessageBox.Show("Service Provider is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await AppHost.StopAsync();
                AppHost.Dispose();
                Shutdown();
                return;
            }

            MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            MainViewModel mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;
            mainViewModel.OnExit += OnMainWindowClosed;

            FileSyncBackgroundWorker fileSyncWorker = serviceProvider.GetRequiredService<FileSyncBackgroundWorker>();

            LoginViewWindow loginWindow = serviceProvider.GetRequiredService<LoginViewWindow>();

            loginWindow.OnDialogClosed += async (result) =>
            {
                if (result == true)
                {
                    mainWindow.Show();
                    await fileSyncWorker.Start();
                }
                else
                {
                    await fileSyncWorker.Stop();
                    await AppHost.StopAsync();
                    AppHost.Dispose();
                    Shutdown();
                    return;
                }
            };

            loginWindow.OpenDialog();

            base.OnStartup(e);
        }

        private async void OnMainWindowClosed()
        {
            if (AppHost == null) throw new Exception("App host is null for some reason");

            FileSyncBackgroundWorker fileSyncWorker = AppHost.Services.GetRequiredService<FileSyncBackgroundWorker>();

            await fileSyncWorker.Stop();
            await AppHost.StopAsync();
            AppHost.Dispose();
            Shutdown();
            return;
        }
    }

}
