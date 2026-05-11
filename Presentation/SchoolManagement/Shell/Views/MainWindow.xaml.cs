using SchoolManagement.Presentation.ViewModels;
using System.Windows;

namespace SchoolManagement.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (DataContext is MainViewModel vm)
            {
                vm.OnExit += OnAppClosed;
            }
        }

        private void OnAppClosed()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.LoadAsync();
            }
        }
    }
}