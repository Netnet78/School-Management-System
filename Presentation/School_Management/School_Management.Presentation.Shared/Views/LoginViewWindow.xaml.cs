using School_Management.Presentation.Shared.ViewModels;
using System.Windows;

namespace School_Management.Presentation.Shared.Views
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
            System.Windows.Application.Current.Shutdown();
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
