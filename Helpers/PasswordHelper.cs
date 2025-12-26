using System.Windows;
using System.Windows.Controls;

namespace Student_Management.Helpers
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
                typeof(string),
                typeof(PasswordHelper),
                new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword",
                typeof(bool),
                typeof(PasswordHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        private static bool _isUpdating = false;

        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value ?? string.Empty);
        }

        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPasswordProperty, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPasswordProperty);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                pb.PasswordChanged -= PasswordChanged;

                if (!_isUpdating)
                    pb.Password = (string)e.NewValue;

                pb.PasswordChanged += PasswordChanged;
            }
        }

        private static void OnBindPasswordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                if ((bool)e.NewValue)
                    pb.PasswordChanged += PasswordChanged;
                else
                    pb.PasswordChanged -= PasswordChanged;
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                _isUpdating = true;
                SetPassword(pb, pb.Password);
                _isUpdating = false;
            }
        }
    }
}
