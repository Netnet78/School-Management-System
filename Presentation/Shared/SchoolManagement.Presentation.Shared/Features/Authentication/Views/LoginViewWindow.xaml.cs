using System.Windows;

namespace SchoolManagement.Presentation.Shared.Features.Authentication.Views
{
    /// <summary>
    /// Interaction logic for LoginViewWindow.xaml
    /// </summary>
    public partial class LoginViewWindow : Window, IDialogWindow
    {
        public LoginViewWindow(LoginViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.LoginSucceeded = (result) =>
            {
                if (result == true)
                {
                    DialogResult = true;
                    Close();
                }
            };
        }

        public event Action<bool?>? OnDialogClosed;
        public event Action? OnDialogOpened;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.LoginCommand.Execute(PasswordBox.Password);
            }
        }

        public bool? OpenDialog(IViewModel? viewModel = null)
        {
            if (viewModel != null)
            {
                DataContext = viewModel;
            }
            bool? result = null;
            try
            {
                OnDialogOpened?.Invoke();
                result = ShowDialog();
                return result;
            }
            finally
            {
                OnDialogClosed?.Invoke(result);
            }
        }
    }
}

