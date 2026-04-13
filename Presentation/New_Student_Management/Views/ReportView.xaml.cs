using New_Student_Management.ViewModels;
using System.Windows.Controls;

namespace New_Student_Management.Views
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
