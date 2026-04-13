using New_Student_Management.ViewModels;
using System.Windows.Controls;

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
