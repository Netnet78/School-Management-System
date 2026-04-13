using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using New_Student_Management.ViewModels;
using New_Student_Management.ViewModels.Factories;
using New_Student_Management.Views;
using New_Student_Management.Views.Wizards;
using New_Student_Management.Views.Wizards.Services;
using School_Management.Application;
using School_Management.Infrastructure;
using School_Management.Presentation.Shared;
using System.Windows;

namespace New_Student_Management
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
                    services.AddSingleton<StudentViewModel>();
                    services.AddTransient<EditStudentViewModel>();
                    services.AddSingleton<InsertStudentViewModel>();
                    services.AddSingleton<ReportViewModel>();
                    services.AddSingleton<LoginViewModel>();

                    // Register ViewModel Factories
                    services.AddSingleton<IEditStudentViewModelFactory, EditStudentViewModelFactory>();

                    // Register UI Services
                    services.AddTransient<IEditStudentWizardService, EditStudentWizardService>();

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
            //var loginWindow = serviceProvider.GetRequiredService<LoginViewWindow>();
            //bool? loginResult = loginWindow.ShowDialog();

            //if (loginResult == true)
            //{
            //    mainWindow.Show();
            //}
            //else
            //{
            //    await AppHost.StopAsync();
            //    AppHost.Dispose();
            //    Shutdown();
            //    return;
            //}

            mainWindow.Show();
            base.OnStartup(e);
        }

    }

}
