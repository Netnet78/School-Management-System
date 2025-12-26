using Student_Management.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Student_Management.Views.Wizards
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
