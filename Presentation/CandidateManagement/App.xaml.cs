using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CandidateManagement.ViewModels;
using CandidateManagement.Views;
using SchoolManagement.Application;
using SchoolManagement.Application.Workers;
using SchoolManagement.Infrastructure;
using SchoolManagement.Presentation.Shared;
using System.Windows;

namespace CandidateManagement
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;
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
                    services.AddSingleton<MainFormViewModel>();
                    services.AddSingleton<StudentViewModel>();
                    services.AddTransient<EditStudentViewModel>();
                    services.AddSingleton<InsertStudentViewModel>();
                    services.AddSingleton<ReportViewModel>();
                    services.AddSingleton<LoginViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                    services.AddTransient<LoginViewWindow>();
                    services.AddTransient<MainFormView>();
                    services.AddTransient<StudentTableView>();
                    services.AddTransient<EditStudentView>();
                    services.AddTransient<InsertStudentView>();
                    services.AddTransient<ReportView>();
                }).Build();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();

            base.OnExit(e);
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

            // create a fresh login window
            MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            MainViewModel mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;
            FileSyncBackgroundWorker fileSyncWorker = serviceProvider.GetRequiredService<FileSyncBackgroundWorker>();
            mainViewModel.ExitAction += OnShutdown;

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
            //mainWindow.Show();
            base.OnStartup(e);
        }

        private async void OnShutdown()
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
