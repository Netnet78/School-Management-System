using CandidateManagement.ViewModels;
using System.Windows;

namespace CandidateManagement.Views
{
    /// <summary>
    /// Interaction logic for LoginViewWindow.xaml
    /// </summary>
    public partial class LoginViewWindow : Window
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
    }
}
