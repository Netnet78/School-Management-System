using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace School_Management.Presentation.Shared.Components
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public TitleBar()
        {
            InitializeComponent();
        }

        public TitleBar(string title)
        {
            InitializeComponent();
            WindowTitle.Text = $"សាលាដុនបូស្កូ សាខាប៉ោយប៉ែត ({title})";
        }

        public TitleBar(string title, bool replace)
        {
            InitializeComponent();
            if (replace == true)
            {
                WindowTitle.Text = $"{title}";
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(TitleBar),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ReplaceProperty =
            DependencyProperty.Register(
                nameof(Replace),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(false));

        public bool Replace
        {
            get => (bool)GetValue(ReplaceProperty);
            set => SetValue(ReplaceProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public void MinimizeButton_MouseDown(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        public void MaximizeButton_MouseDown(object sender, RoutedEventArgs e)
        {
            WindowState currentWindowState = Window.GetWindow(this).WindowState;
            if (currentWindowState is WindowState.Maximized)
            {
                Window.GetWindow(this).WindowState = WindowState.Normal;
            }
            else
            {
                Window.GetWindow(this).WindowState = WindowState.Maximized;
            }
        }

        public void CloseButton_MouseDown(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current?.Shutdown();
        }

        private void TitlebarPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
            }
            else if (e.ButtonState == MouseButtonState.Pressed)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    try { window.DragMove(); } catch { }
                }
            }
        }
    }
}
