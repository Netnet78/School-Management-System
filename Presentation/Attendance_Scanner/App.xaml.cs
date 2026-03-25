using Attendance_Scanner.Services;
using Attendance_Scanner.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using School_Management.Infrastructure.Data;
using School_Management.Infrastructure.Repositories;
using School_Management.Presentation.Shared.Components;
using System.Configuration;
using System.Data;
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

                string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");

                // Use Npgsql with connection pooling for better performance
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IStudentQRRepository, StudentQRRepository>();

            services.AddSingleton<IAttendanceQRService, AttendanceQRService>();
            services.AddSingleton<IMessageService, MessageService>();

            services.AddSingleton<MainViewModel>();

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
