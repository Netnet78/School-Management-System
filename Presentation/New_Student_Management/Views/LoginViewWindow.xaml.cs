using New_Student_Management.ViewModels;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Enums;
using System.Windows;

namespace New_Student_Management.Views
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
    }
}
