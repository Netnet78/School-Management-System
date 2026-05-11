using SchoolManagement.Core.Enums;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SchoolManagement.Presentation.Shared.Components
{

    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    /// 
    public partial class CustomMessageBox : Window
    {

        public MessageResult Result { get; private set; }

        private readonly MessageButton[] OKButtons = [MessageButton.OK, MessageButton.OKCancel];
        private readonly MessageButton[] CancelButtons = [
            MessageButton.CancelTryContinue,
            MessageButton.OKCancel,
            MessageButton.RetryCancel,
            MessageButton.CancelTryContinue,
            MessageButton.YesNoCancel,
        ];
        private readonly MessageButton[] YesNoButtons = [
            MessageButton.YesNo,
            MessageButton.YesNoCancel,
        ];
        private readonly MessageButton[] RetryButtons = [
            MessageButton.RetryCancel,
            MessageButton.AbortRetry,
        ];
        private readonly MessageButton[] AbortButtons = [
            MessageButton.AbortRetry
        ];

        public CustomMessageBox(string title, string message, MessageButton messageBoxButton, MessageIcon messageBoxImage, int? autoHideIn = null)
        {
            InitializeComponent();

            TitleText.Text = title;
            message = message
                .Replace("\\r\\n", "\r\n")
                .Replace("\\n", "\n");
            MessageText.Text = message;

            // Button section
            if (OKButtons.Contains(messageBoxButton))
            {
                OKButton.Visibility = Visibility.Visible;
            }
            if (CancelButtons.Contains(messageBoxButton))
            {
                CancelButton.Visibility = Visibility.Visible;
            }
            if (YesNoButtons.Contains(messageBoxButton))
            {
                YesButton.Visibility = Visibility.Visible;
                NoButton.Visibility = Visibility.Visible;
            }
            if (RetryButtons.Contains(messageBoxButton))
            {
                RetryButton.Visibility = Visibility.Visible;
            }
            if (AbortButtons.Contains(messageBoxButton))
            {
                AbortButton.Visibility = Visibility.Visible;
            }

            // Icon (Image) section
            switch (messageBoxImage)
            {
                case MessageIcon.None:
                    MessageViewIcon.Visibility = Visibility.Collapsed;
                    IconBorder.Visibility = Visibility.Collapsed;
                    break;
                case MessageIcon.Exclamation:
                    MessageViewIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ExclamationBold;
                    MessageViewIcon.Foreground = new SolidColorBrush(Colors.Black);
                    IconBorder.Background = new SolidColorBrush(Colors.Yellow);
                    break;
                case MessageIcon.Question:
                    MessageViewIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.QuestionMark;
                    break;
                case MessageIcon.Information:
                    MessageViewIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Information;
                    break;
                case MessageIcon.Hand:
                    MessageViewIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Hand;
                    break;
                case MessageIcon.Success:
                    MessageViewIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Success;
                    IconBorder.Background = new SolidColorBrush(Colors.DarkGreen);
                    break;
                case MessageIcon.Error:
                    MessageViewIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Error;
                    IconBorder.Background = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    MessageViewIcon.Visibility = Visibility.Collapsed;
                    break;
            }

            // Auto hide logic
            if (autoHideIn != null)
            {
                DispatcherTimer timer = new()
                {
                    Interval = TimeSpan.FromSeconds(autoHideIn.Value)
                };

                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    Result = MessageResult.None;
                    Close();
                };

                timer.Start();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DragMove();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageResult.Cancel;
            Close();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageResult.Yes;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageResult.No;
            Close();
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageResult.Retry;
            Close();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageResult.Abort;
            Close();
        }

        public static MessageResult Show(string message, string title, MessageButton button, MessageIcon icon, int? autoHide = null)
        {
            CustomMessageBox msg = new(title, message, button, icon, autoHide)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            msg.ShowDialog();
            return msg.Result;
        }
    }
}
