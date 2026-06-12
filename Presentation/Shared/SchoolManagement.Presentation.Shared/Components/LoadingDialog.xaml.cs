using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SchoolManagement.Presentation.Shared.Components
{
    /// <summary>
    /// Interaction logic for LoadingMessageDialog.xaml
    /// </summary>
    public partial class LoadingDialog : UserControl
    {
        private readonly Storyboard _progressCogRotateAnimation;

        public LoadingDialog()
        {
            InitializeComponent();

            _progressCogRotateAnimation = GetAnimation("ProgressCogRotateAnimation");
        }
        public string LoadingText
        {
            get => (string)GetValue(LoadingTitleProperty);
            set => SetValue(LoadingTitleProperty, value);
        }

        public static readonly DependencyProperty LoadingTitleProperty =
            DependencyProperty.Register(
                nameof(LoadingText),
                typeof(string),
                typeof(LoadingDialog),
                new PropertyMetadata("កំពុងដំណើរការទិន្នន័យ! សូមមេត្តារងចាំ...")
            );

        public LoadingState State
        {
            get => (LoadingState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                nameof(State),
                typeof(LoadingState),
                typeof(LoadingDialog),
                new(LoadingState.None, OnStateChanged)
            );

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LoadingDialog control = (LoadingDialog)d;
            LoadingState value = (LoadingState)e.NewValue;

            switch (value)
            {
                case LoadingState.Loading:
                    VisualStateManager.GoToState(control, "Loading", true);
                    control.ButtonSection.Visibility = Visibility.Collapsed;
                    control.LoadingIndicator.Visibility = Visibility.Visible;
                    control.ProgressIndicator.Visibility = Visibility.Collapsed;
                    control.ErrorIndicator.Visibility = Visibility.Collapsed;
                    control.SuccessIndicator.Visibility = Visibility.Collapsed;
                    break;
                case LoadingState.Progress:
                    VisualStateManager.GoToState(control, "Progress", true);
                    control.ButtonSection.Visibility = Visibility.Collapsed;
                    control.LoadingIndicator.Visibility = Visibility.Collapsed;
                    control.ProgressIndicator.Visibility = Visibility.Visible;
                    control.ErrorIndicator.Visibility = Visibility.Collapsed;
                    control.SuccessIndicator.Visibility = Visibility.Collapsed;
                    break;
                case LoadingState.Success:
                    VisualStateManager.GoToState(control, "Success", true);
                    control.ButtonSection.Visibility = Visibility.Visible;
                    control.LoadingIndicator.Visibility = Visibility.Collapsed;
                    control.ProgressIndicator.Visibility = Visibility.Collapsed;
                    control.ErrorIndicator.Visibility = Visibility.Collapsed;
                    control.SuccessIndicator.Visibility = Visibility.Visible;
                    break;
                case LoadingState.Error:
                    VisualStateManager.GoToState(control, "Error", true);
                    control.ButtonSection.Visibility = Visibility.Visible;
                    control.LoadingIndicator.Visibility = Visibility.Collapsed;
                    control.ProgressIndicator.Visibility = Visibility.Collapsed;
                    control.ErrorIndicator.Visibility = Visibility.Visible;
                    control.SuccessIndicator.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        public double ProgressValue
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly DependencyProperty ProgressProperty = 
            DependencyProperty.Register(
                nameof(ProgressValue),
                typeof(double),
                typeof(LoadingDialog),
                new PropertyMetadata(0.0, OnProgressChanged)
            );

        private static async void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LoadingDialog control = (LoadingDialog)d;
            double value = (double)e.NewValue;

            control.StartAnimation(control._progressCogRotateAnimation);

            control.LeftStop.Offset = value / 100;
            control.RightStop.Offset = Math.Min(1, (value / 100) + 0.1);

            if (value == 0)
            {
                control._progressCogRotateAnimation?.SetSpeedRatio(0);
                return;
            }
            if (value <= 20)
            {
                control._progressCogRotateAnimation?.SetSpeedRatio(0.2);
                return;
            }
            if (value <= 40)
            {
                control._progressCogRotateAnimation?.SetSpeedRatio(0.5);
                return;
            }
            if (value <= 60)
            {
                control._progressCogRotateAnimation?.SetSpeedRatio(0.8);
                return;
            }
            if (value <= 80)
            {
                control._progressCogRotateAnimation?.SetSpeedRatio(1.25);
                return;
            }
            if (value < 100)
            {
                control._progressCogRotateAnimation?.SetSpeedRatio(1.5);
                return;
            }

            if (value >= 100)
            {
                await Task.Delay(1000);
                control._progressCogRotateAnimation?.Stop();
                control.State = LoadingState.Success;
                return;
            }
        }

        private Storyboard GetAnimation(string resourceName)
        {
            Storyboard storyboard = (Storyboard)FindResource(resourceName);
            return storyboard;
        }

        private void StartAnimation(Storyboard storyboard)
        {
            storyboard.Begin(this, true);
        }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => Window.GetWindow(this)?.Close());
        }
    }
}
