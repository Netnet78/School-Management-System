using AttendanceScanner.Services;
using AttendanceScanner.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Application;
using SchoolManagement.Application.Workers;
using SchoolManagement.Core.Shared.Presentation.Contracts;
using SchoolManagement.Core.Shared.Configurations;
using SchoolManagement.Infrastructure;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Presentation.Shared.Services;
using SchoolManagement.Presentation.Shared.ViewModels;
using SchoolManagement.Presentation.Shared.Views;
using System.IO;
using System.Windows;

namespace AttendanceScanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;
        public IServiceProvider? ServiceProvider { get; }

        public App()
        {
            ServiceCollection services = new();

            // Register DbContext as singleton for WPF
            services.AddDbContext<SchoolDbContext>(options =>
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddUserSecrets<SchoolDbContext>()
                    .Build();

                string connectionString = SecretHelper.GetValueFromEnv("DB_CONNECTION");

                // Use Npgsql with connection pooling for better performance
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });
            });

            // Infrastructure repositories
            services.AddInfrastructure();

            // Application services
            services.AddApplication();

            // Presentation services
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<ISoundService, SoundService>();
            services.AddSingleton<ICameraService, CameraService>();
            services.AddSingleton<IFrameProcessingService, FrameProcessingService>();
            services.AddSingleton<IQRScannerService, QRScannerService>();
            services.AddSingleton<IDispatcherService, DispatcherService>();
            services.AddSingleton<ILoadingService, LoadingService>();

            // Project services
            services.AddSingleton<IAttendanceQRService, AttendanceQRService>();

            // Project view models
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoginViewModel>();

            // Project views
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginViewWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            if (ServiceProvider == null)
            {
                MessageBox.Show("Service Provider is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Closed += OnMainWindowClosed;

            var loginWindow = ServiceProvider.GetRequiredService<LoginViewWindow>();

            var fileSyncWorker = ServiceProvider.GetRequiredService<FileSyncBackgroundWorker>();


            bool? loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
            {
                mainWindow.Show();
                await fileSyncWorker.Start();
            }
            else
            {
                await fileSyncWorker.Stop();
                Shutdown();
                return;
            }


            base.OnStartup(e);
        }

        private async void OnMainWindowClosed(object? sender, EventArgs e)
        {
            var fileSyncWorker = ServiceProvider!.GetRequiredService<FileSyncBackgroundWorker>();
            await fileSyncWorker.Stop();
        }
    }

}
