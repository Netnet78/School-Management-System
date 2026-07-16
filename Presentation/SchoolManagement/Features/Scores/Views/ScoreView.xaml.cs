using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Presentation.Features.Scores.ViewModels;

namespace SchoolManagement.Presentation.Features.Scores.Views
{
    public partial class ScoreView : UserControl
    {
        public ScoreView()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ScoreViewModel oldVm)
            {
                oldVm.Mappers.CollectionChanged -= OnMappersChanged;
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            }

            if (e.NewValue is ScoreViewModel newVm)
            {
                newVm.Mappers.CollectionChanged += OnMappersChanged;
                newVm.PropertyChanged += OnViewModelPropertyChanged;
                RebuildComponentColumns(newVm.Mappers);
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScoreViewModel.Mappers) && sender is ScoreViewModel vm)
            {
                RebuildComponentColumns(vm.Mappers);
            }
        }

        private void OnMappersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (DataContext is ScoreViewModel vm)
            {
                RebuildComponentColumns(vm.Mappers);
            }
        }

        private void RebuildComponentColumns(ObservableCollection<SubjectMapper> mappers)
        {
            ScoreGrid.Columns.Clear();

            // Static: Student Name
            ScoreGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ឈ្មោះសិស្ស",
                Binding = new Binding("Student.FullName"),
                Width = 220,
                IsReadOnly = true
            });

            // Dynamic: one column per component
            for (int i = 0; i < mappers.Count; i++)
            {
                var mapper = mappers[i];
                int index = i;

                var templateColumn = new DataGridTemplateColumn
                {
                    Header = mapper.Component?.KhmerName ?? $"សមាសភាគ {i + 1}",
                    Width = 120
                };

                var factory = new FrameworkElementFactory(typeof(TextBox));
                factory.SetValue(TextBox.PaddingProperty, new System.Windows.Thickness(4));
                factory.SetValue(TextBox.FontSizeProperty, 14.0);
                factory.SetValue(TextBox.WidthProperty, 90.0);
                factory.SetValue(TextBox.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Left);

                string bindingPath = $"ComponentScores[{index}].ScoreAmount";
                factory.SetBinding(TextBox.TextProperty, new Binding(bindingPath)
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

                templateColumn.CellTemplate = new DataTemplate { VisualTree = factory };
                ScoreGrid.Columns.Add(templateColumn);
            }
        }

        private void ScoreGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;

                // Move focus to the cell below
                if (System.Windows.Input.Keyboard.FocusedElement is UIElement element)
                {
                    element.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Down));
                }
            }
        }
    }
}