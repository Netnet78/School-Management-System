using CandidateManagement.ViewModels;
using System.Windows.Controls;

namespace CandidateManagement.Views
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
