using CandidateManagement.ViewModels;
using System.Windows.Controls;

namespace CandidateManagement.Views
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();

            if (DataContext is ReportViewModel viewModel)
            {

            }
        }

    }
}
