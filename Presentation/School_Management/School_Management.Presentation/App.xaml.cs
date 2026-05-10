using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using School_Management.Application;
using School_Management.Application.Workers;
using School_Management.Infrastructure;
using School_Management.Presentation.Shared;
using School_Management.Presentation.Shared.ViewModels;
using School_Management.Presentation.Shared.Views;
using School_Management.Presentation.ViewModels;
using School_Management.Presentation.Views;
using System.Windows;

namespace School_Management.Presentation
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
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<DashboardViewModel>();
                    services.AddSingleton<AttendanceViewModel>();
                    services.AddSingleton<ClassViewModel>();
                    services.AddSingleton<StudentListViewModel>();
                    services.AddSingleton<ReportViewModel>();
                    services.AddSingleton<LoginViewModel>();
                    services.AddSingleton<EmployeeViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                    services.AddTransient<DashboardView>();
                    services.AddTransient<AttendanceView>();
                    services.AddTransient<ClassView>();
                    services.AddTransient<StudentListView>();
                    services.AddTransient<ReportView>();
                    services.AddTransient<EmployeeView>();
                    services.AddTransient<LoginViewWindow>();
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
