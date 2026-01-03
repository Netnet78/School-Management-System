using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace New_Student_Management.Components
{
    /// <summary>
    /// Interaction logic for StudentDataGrid.xaml
    /// </summary>
    public partial class StudentDataGrid : UserControl
    {
        public StudentDataGrid()
        {
            InitializeComponent();
        }

        // ItemsSource prop
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(StudentDataGrid));

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // SelectedItem prop
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(StudentDataGrid),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
    }
}
