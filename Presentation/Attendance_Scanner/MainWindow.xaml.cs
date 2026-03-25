using Attendance_Scanner.ViewModels;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Attendance_Scanner
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

            if (DeviceSelector.DisplayMemberPath.Length > 0)
            {
                DeviceSelector.SelectedIndex = 0;
            }

            timer.Interval = TimeSpan.FromSeconds(1.5);
            timer.Tick += Timer_Tick!;
            timer.Start();
        }

        // Indicator live button
        private bool isRed = true;
        DispatcherTimer timer = new DispatcherTimer();
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isRed)
            {
                Indicator.Fill = Brushes.White;
            }
            else
            {
                Indicator.Fill = Brushes.Red;
            }
            isRed = !isRed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AppClosingCommand.Execute(null);
            }
        }
    }
}