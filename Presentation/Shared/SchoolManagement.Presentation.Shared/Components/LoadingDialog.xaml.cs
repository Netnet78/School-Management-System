using System.Windows;
using System.Windows.Controls;

namespace SchoolManagement.Presentation.Shared.Components
{
    /// <summary>
    /// Interaction logic for LoadingMessageDialog.xaml
    /// </summary>
    public partial class LoadingDialog : UserControl
    {
        public LoadingDialog()
        {
            InitializeComponent();
        }

        public string LoadingText
        {
            get => (string)GetValue(LoadingTitleProperty);
            set => SetValue(LoadingTitleProperty, value);
        }

        public static readonly DependencyProperty LoadingTitleProperty =
            DependencyProperty.Register(
                nameof(LoadingText),
                typeof(string),
                typeof(LoadingDialog),
                new PropertyMetadata("កំពុងដំណើរការទិន្នន័យ! សូមមេត្តារងចាំ...")
            );
    }
}
