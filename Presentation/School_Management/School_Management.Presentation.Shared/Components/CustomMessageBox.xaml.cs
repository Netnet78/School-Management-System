using System.Windows;

namespace School_Management.Presentation.Shared.Components
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    /// 
    public enum MessageBoxIcon
    {
        None,
        Exclamation,
        Question,
        Information,
        Hand,
        Success,
        Error,
    }

    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public MessageBoxResult Result { get; private set; }

        private readonly MessageBoxButton[] OKButtons = [MessageBoxButton.OK, MessageBoxButton.OKCancel];
        private readonly MessageBoxButton[] CancelButtons = [
            MessageBoxButton.CancelTryContinue, 
            MessageBoxButton.OKCancel, 
            MessageBoxButton.RetryCancel,
            MessageBoxButton.CancelTryContinue,
            MessageBoxButton.YesNoCancel,
        ];
        private readonly MessageBoxButton[] YesNoButtons = [
            MessageBoxButton.YesNo,
            MessageBoxButton.YesNoCancel,
        ];
        private readonly MessageBoxButton[] RetryButtons = [
            MessageBoxButton.RetryCancel,
            MessageBoxButton.AbortRetryIgnore,
        ];
        private readonly MessageBoxButton[] AbortButtons = [
            MessageBoxButton.AbortRetryIgnore
        ];

        public CustomMessageBox(string title, string message, MessageBoxButton messageBoxButton, MessageBoxIcon messageBoxImage)
        {
            InitializeComponent();

            TitleText.Text = title;
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
                case MessageBoxIcon.None:
                    Icon.Visibility = Visibility.Collapsed;
                    IconBorder.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxIcon.Exclamation:
                    Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Exclamation;
                    break;
                case MessageBoxIcon.Question:
                    Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.QuestionMark;
                    break;
                case MessageBoxIcon.Information:
                    Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Information;
                    break;
                case MessageBoxIcon.Hand:
                    Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Hand;
                    break;
                case MessageBoxIcon.Success:
                    Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Success;
                    break;
                case MessageBoxIcon.Error:
                    Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Error;
                    break;
                default:
                    Icon.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DragMove();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Retry;
            Close();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Abort;
            Close();
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton messageBoxButton, MessageBoxIcon messageBoxImage)
        {
            CustomMessageBox msg = new(title, message, messageBoxButton, messageBoxImage);
            msg.ShowDialog();
            return msg.Result;
        }
    }

    public static class MessageService
    {
        public static MessageBoxResult Show(
            string message, 
            string title = "Message", 
            MessageBoxButton button = MessageBoxButton.OK, 
            MessageBoxIcon icon = MessageBoxIcon.None
        )
        {
            return CustomMessageBox.Show(message, title, button, icon);
        }
    }
}
