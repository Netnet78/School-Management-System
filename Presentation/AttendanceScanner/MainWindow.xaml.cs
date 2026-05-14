using AttendanceScanner.ViewModels;
using SchoolManagement.Presentation.Shared.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AttendanceScanner
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

            vm.OnScanStatusChanged += Vm_OnQrScanned;

            _indicatorTimer.Interval = TimeSpan.FromSeconds(1.5);
            _indicatorTimer.Tick += Timer_Tick!;
            _indicatorTimer.Start();

            //StartLoadingAnimation();
        }

        private void Vm_OnQrScanned(ReturnResponse response)
        {
            double openingDuration = 0.3;
            double closingDuration = 0.3;
            double visibleTime = 12;

            Dispatcher.BeginInvoke(() =>
            {
                Storyboard storyboard = new();

                bool mainContentColumnIsOpen = MainContentFirstGridDefinition.Width.Value > 0;

                // Animations for the time when the scan is successful
                DoubleAnimation messageOpenAnimation = new()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(openingDuration)
                };

                DoubleAnimation messageCloseAnimation = new()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(closingDuration),
                    BeginTime = TimeSpan.FromSeconds(openingDuration + 3),
                };

                GridLengthAnimation openingAnimation = new()
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(1, GridUnitType.Star),
                    Duration = TimeSpan.FromSeconds(openingDuration)
                };

                GridLengthAnimation immediateCloseAnimation = new()
                {
                    From = new GridLength(1, GridUnitType.Star),
                    To = new GridLength(0, GridUnitType.Star),
                    Duration = TimeSpan.FromSeconds(closingDuration),
                    BeginTime = TimeSpan.Zero
                };

                GridLengthAnimation closingAnimation = new()
                {
                    From = new GridLength(1, GridUnitType.Star),
                    To = new GridLength(0, GridUnitType.Star),
                    Duration = TimeSpan.FromSeconds(closingDuration),
                    BeginTime = TimeSpan.FromSeconds(openingDuration + visibleTime)
                };

                if (response.Status == Status.Success)
                {

                    if (mainContentColumnIsOpen)
                    {
                        openingAnimation.BeginTime = TimeSpan.FromSeconds(closingDuration);
                        AddAnimationToStoryboard(storyboard, immediateCloseAnimation, MainContentFirstGridDefinition, ColumnDefinition.WidthProperty);
                    }

                    AddAnimationToStoryboard(storyboard, openingAnimation, MainContentFirstGridDefinition, ColumnDefinition.WidthProperty);
                    AddAnimationToStoryboard(storyboard, closingAnimation, MainContentFirstGridDefinition, ColumnDefinition.WidthProperty);
                }
                else if (response.Status == Status.Pending)
                {
                    StartLoadingAnimation();
                }

                storyboard.Begin();
            });
        }

        private static void AddAnimationToStoryboard(Storyboard storyboard, Timeline animation, DependencyObject target, object property)
        {
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
        }

        // Indicator live button
        private bool isRed = true;
        private readonly DispatcherTimer _indicatorTimer = new();

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

        private void StartLoadingAnimation()
        {
            // Loading service
        }

        private void StopLoadingAnimation()
        {
            
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AppClosingCommand.Execute(null);
            }

            _indicatorTimer.Stop();
            _indicatorTimer.Tick -= Timer_Tick!;
        }
    }
}