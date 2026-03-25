using New_Student_Management.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace New_Student_Management.Views
{
    /// <summary>
    /// Interaction logic for InsertStudentView.xaml
    /// </summary>
    public partial class InsertStudentView : UserControl
    {
        public InsertStudentView()
        {
            InitializeComponent();

            if (DataContext is InsertStudentViewModel viewModel)
            {

            }
        }
    }
}
