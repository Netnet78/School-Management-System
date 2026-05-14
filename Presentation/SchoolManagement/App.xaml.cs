using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolManagement.Application;
using SchoolManagement.Infrastructure;
using SchoolManagement.Presentation.Features.Attendance.ViewModels;
using SchoolManagement.Presentation.Features.Attendance.Views;
using SchoolManagement.Presentation.Features.AuditLogs.ViewModels;
using SchoolManagement.Presentation.Features.AuditLogs.Views;
using SchoolManagement.Presentation.Features.Classes.ViewModels;
using SchoolManagement.Presentation.Features.Classes.Views;
using SchoolManagement.Presentation.Features.Dashboard.ViewModels;
using SchoolManagement.Presentation.Features.Dashboard.Views;
using SchoolManagement.Presentation.Features.Employees.ViewModels;
using SchoolManagement.Presentation.Features.Employees.Views;
using SchoolManagement.Presentation.Features.Reports.ViewModels;
using SchoolManagement.Presentation.Features.Reports.Views;
using SchoolManagement.Presentation.Features.Students.ViewModels;
using SchoolManagement.Presentation.Features.Students.Views;
using SchoolManagement.Presentation.Shell.ViewModels;
using SchoolManagement.Presentation.Shell.Views;
using System.Windows;

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
                    services.AddPresentationShared();
                    services.AddInfrastructure();
                    services.AddApplication();

                    // Register ViewModels
                    services.AddScoped<MainViewModel>();
                    services.AddScoped<DashboardViewModel>();
                    services.AddScoped<AttendanceViewModel>();
                    services.AddScoped<ClassViewModel>();
                    services.AddScoped<StudentListViewModel>();
                    services.AddScoped<ReportViewModel>();
                    services.AddScoped<LoginViewModel>();
                    services.AddScoped<EmployeeViewModel>();
                    services.AddScoped<AuditLogViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                    services.AddTransient<DashboardView>();
                    services.AddTransient<AttendanceView>();
                    services.AddTransient<ClassView>();
                    services.AddTransient<StudentListView>();
                    services.AddTransient<ReportView>();
                    services.AddTransient<EmployeeView>();
                    services.AddTransient<LoginViewWindow>();
                    services.AddTransient<AuditLogView>();
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

            var loginWindow = serviceProvider.GetRequiredService<LoginViewWindow>();
            bool? loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
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
