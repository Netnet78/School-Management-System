using System.Windows;
using System.Windows.Threading;

namespace SchoolManagement.Presentation.Shared.Components
{
    public partial class LoadingWindow : Window
    {
        private DispatcherTimer? _autoCloseTimer;

        public LoadingWindow()
        {
            InitializeComponent();
        }

        public void SetState(LoadingState state, string message)
        {
            Dialog.State = state;
            Dialog.LoadingText = message;
            Dialog.ProgressValue = 0;

            StopAutoCloseTimer();

            if (state is LoadingState.Success or LoadingState.Error)
            {
                StartAutoCloseTimer();
            }
        }

        public void SetProgress(double value, string? message = null)
        {
            Dialog.State = LoadingState.Progress;
            Dialog.ProgressValue = value;
            if (message != null)
                Dialog.LoadingText = message;

            StopAutoCloseTimer();
        }

        private void StartAutoCloseTimer()
        {
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3.5)
            };
            _autoCloseTimer.Tick += (s, e) =>
            {
                StopAutoCloseTimer();
                Close();
            };
            _autoCloseTimer.Start();
        }

        private void StopAutoCloseTimer()
        {
            if (_autoCloseTimer != null)
            {
                _autoCloseTimer.Stop();
                _autoCloseTimer = null;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            StopAutoCloseTimer();
            base.OnClosed(e);
        }
    }
}
