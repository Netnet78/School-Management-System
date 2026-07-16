using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SchoolManagement.Presentation.Shared.Components
{
    public partial class LoadingPanel : UserControl
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                nameof(IsLoading),
                typeof(bool),
                typeof(LoadingPanel),
                new PropertyMetadata(false, OnIsLoadingChanged));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(LoadingPanel),
                new PropertyMetadata("សូមរងចាំមួយភ្លេទសិន..."));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public LoadingPanel()
        {
            InitializeComponent();
        }

        private async static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LoadingPanel panel = (LoadingPanel)d;
            bool isLoading = (bool)e.NewValue;

            // Get the storyboard from resources
            Storyboard storyboard = isLoading
                ? (Storyboard)panel.Resources["ShowLoading"]
                : (Storyboard)panel.Resources["HideLoading"];

            if (storyboard != null)
            {
                Storyboard clonedStoryboard = storyboard.Clone();

                clonedStoryboard.Begin(panel.LoadingGrid, HandoffBehavior.SnapshotAndReplace);
            }
        }
    }
}