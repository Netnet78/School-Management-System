using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using New_Student_Management.ViewModels;
using New_Student_Management.Views;
using New_Student_Management.Views.Wizards;
using School_Management.Application;
using School_Management.Infrastructure;
using School_Management.Presentation.Shared;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Enums;
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
                    services.AddScoped<StudentViewModel>();
                    services.AddSingleton<EditStudentViewModel>();
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

            if (ServiceProvider == null)
            {
                MessageBox.Show("Service Provider is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // create a fresh login window
            MainWindow mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            //var loginWindow = ServiceProvider.GetRequiredService<LoginViewWindow>();
            //bool? loginResult = loginWindow.ShowDialog();

            //if (loginResult == true)
            //{
            //    mainWindow.Show();
            //}
            //else
            //{
            //    Shutdown();
            //    return;
            //}
            mainWindow.Show();
            base.OnStartup(e);
        }

    }

}
