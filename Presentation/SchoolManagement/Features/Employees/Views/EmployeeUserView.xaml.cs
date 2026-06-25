using System.Windows;
using System.Windows.Controls;
using SchoolManagement.Presentation.Features.Employees.ViewModels;

namespace SchoolManagement.Presentation.Features.Employees.Views
{
    public partial class EmployeeUserView : UserControl
    {
        public EmployeeUserView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeUserViewModel vm)
            {
                vm.GetCreatePassword = () => NewPasswordBox.Password;
                vm.GetCreateConfirmPassword = () => ConfirmPasswordBox.Password;
                vm.GetResetPassword = () => ResetPasswordBox.Password;
                vm.GetResetConfirmPassword = () => ResetConfirmPasswordBox.Password;
            }
        }
    }
}
