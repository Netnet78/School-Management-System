using System.Windows;
using System.Windows.Controls;

namespace SchoolManagement.Presentation.Shared.Components
{
    /// <summary>
    /// Interaction logic for SummaryContent.xaml
    /// </summary>
    public partial class SummaryContent : UserControl
    {
        public SummaryContent()
        {
            InitializeComponent();
        }

        public string Summary
        {
            get => (string)GetValue(SummaryProperty);
            set => SetValue(SummaryProperty, value);
        }

        public static readonly DependencyProperty SummaryProperty =
            DependencyProperty.Register(
                nameof(Summary),
                typeof(string),
                typeof(SummaryContent),
                new PropertyMetadata(string.Empty)
            );
    }
}
