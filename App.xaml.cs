using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using New_Student_Management.Data;
using New_Student_Management.Models;
using New_Student_Management.Services;
using New_Student_Management.ViewModels;
using New_Student_Management.Views;
using New_Student_Management.Views.Wizards;
using System.Windows;

namespace New_Student_Management
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
            services.AddDbContext<StudentDbContext>();

            // Register Repositories
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Register Services
            services.AddSingleton<IUserSessionService, UserSessionService>();

            // Register ViewModels
            services.AddScoped<MainViewModel>();
            services.AddScoped<StudentViewModel>();
            services.AddTransient<EditStudentViewModel>();
            services.AddScoped<InsertStudentViewModel>();
            services.AddScoped<ReportViewModel>();
            services.AddScoped<LoginViewModel>();

            // Register Views
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginViewWindow>();
            services.AddTransient<StudentTableView>();
            services.AddTransient<EditStudentWizard>();
            services.AddTransient<InsertStudentView>();
            services.AddTransient<ReportView>();

            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (ServiceProvider == null)
            {
                MessageBox.Show("Service Provider is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // create a fresh login window
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            var loginWindow = ServiceProvider.GetRequiredService<LoginViewWindow>();
            bool? loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
            {
                mainWindow.Show();
            }
            else
            {
                Shutdown();
                return;
            }
            mainWindow.Show();
            base.OnStartup(e);
        }

    }

}
