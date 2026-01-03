using New_Student_Management.ViewModels;
using System.Windows;

namespace New_Student_Management.Views.Wizards
{
    /// <summary>
    /// Interaction logic for EditStudentWizard.xaml
    /// </summary>
    public partial class EditStudentWizard : Window
    {
        public EditStudentWizard(EditStudentViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            vm.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
        }
    }
}
