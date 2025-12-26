using Student_Management.ViewModels;
using System.Windows;

namespace Student_Management
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;

            vm.ExitAction += () =>
            {
                Application.Current.Shutdown();
            };
        }
    }
}