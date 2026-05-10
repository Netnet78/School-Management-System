using Attendance_Scanner.Services;
using Attendance_Scanner.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using School_Management.Application;
using School_Management.Application.Workers;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Infrastructure;
using School_Management.Infrastructure.Data;
using School_Management.Presentation.Shared.Services;
using School_Management.Presentation.Shared.ViewModels;
using School_Management.Presentation.Shared.Views;
using System.IO;
using System.Windows;

namespace Attendance_Scanner
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

                string connectionString = Env.Get("DB_CONNECTION");

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
