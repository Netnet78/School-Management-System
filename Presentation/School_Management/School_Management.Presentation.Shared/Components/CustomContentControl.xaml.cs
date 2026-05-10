using School_Management.Core.Interfaces.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace School_Management.Presentation.Shared.Components
{
    /// <summary>
    /// Interaction logic for CustomContentControl.xaml
    /// </summary>
    public partial class CustomContentControl : UserControl
    {
        private Storyboard? _contentFadeIn;
        private Storyboard? _contentFadeOut;
        private bool _isInitialized = false;

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                nameof(IsLoading),
                typeof(bool),
                typeof(CustomContentControl),
                new(false)
            );

        public IViewModel? CurrentView
        {
            get => (IViewModel?)GetValue(CurrentViewProperty);
            set => SetValue(CurrentViewProperty, value);
        }

        public static readonly DependencyProperty CurrentViewProperty =
            DependencyProperty.Register(
                nameof(CurrentView),
                typeof(IViewModel),
                typeof(CustomContentControl),
                new PropertyMetadata(null, OnCurrentViewChanged)
            );

        private async static void OnCurrentViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomContentControl control = (CustomContentControl)d;
            IViewModel? value = (IViewModel?)e.NewValue;

            if (!control._isInitialized || control._contentFadeIn == null || control._contentFadeOut == null)
            {
                //control.ContentContainer.Content = value;
                return;
            }

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Storyboard fadeOut = control._contentFadeOut.Clone();
                fadeOut.Begin(control.ContentContainer, HandoffBehavior.SnapshotAndReplace);
            });
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Storyboard fadeIn = control._contentFadeIn.Clone();
                fadeIn.Begin(control.ContentContainer, HandoffBehavior.SnapshotAndReplace);
            });
        }

        public CustomContentControl()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                _contentFadeIn = (Storyboard)Resources["ContentFadeIn"];
                _contentFadeOut = (Storyboard)Resources["ContentFadeOut"];
                _isInitialized = true;
            };
        }

    }
}
