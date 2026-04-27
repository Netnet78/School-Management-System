using Attendance_Scanner.Services;
using Attendance_Scanner.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using School_Management.Application.Services;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Infrastructure.Data;
using School_Management.Infrastructure.Repositories;
using School_Management.Infrastructure.Services;
using School_Management.Presentation.Shared.Services;
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
            services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IAttendanceRepository, AttendanceRepository>();
            services.AddSingleton<IStudentRepository, StudentRepository>();
            services.AddSingleton<IStudentClassRepository, StudentClassRepository>();
            services.AddSingleton<IStudentQRRepository, StudentQRRepository>();

            // Application services
            services.AddSingleton<IUserValidationService, UserValidationService>();
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<IPhotoFetchService, PhotoFetchService>();

            // Infrastructure services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IS3Service, S3Service>();

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

            // Project views
            services.AddTransient<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (ServiceProvider == null)
            {
                MessageBox.Show("Service Provider is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }

}
