using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Converters;
using SchoolManagement.Presentation.Features.Reports.Models;
using SchoolManagement.Presentation.Shared.Components;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace SchoolManagement.Presentation.Features.Reports.Views
{
    public partial class GroupedReportTablePreviewView : UserControl
    {
        private static readonly BytesToImageConverter _bytesToImageConverter = new();
        private IGroupedReportTablePreviewProvider? _previewProvider;

        public GroupedReportTablePreviewView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_previewProvider != null)
            {
                _previewProvider.PropertyChanged -= OnPreviewProviderPropertyChanged;
                _previewProvider = null;
            }

            if (e.NewValue is IGroupedReportTablePreviewProvider provider)
            {
                _previewProvider = provider;
                _previewProvider.PropertyChanged += OnPreviewProviderPropertyChanged;
            }
        }

        private void OnPreviewProviderPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IGroupedReportTablePreviewProvider.GroupedTableData))
                return;

            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(RebuildTabs));
        }

        private void RebuildTabs()
        {
            GroupTabControl.Items.Clear();

            if (_previewProvider?.GroupedTableData == null)
                return;

            foreach (var tab in _previewProvider.GroupedTableData.Tabs)
            {
                var tabItem = new TabItem
                {
                    Header = tab.Header,
                    Content = BuildTabContent(tab),
                };
                GroupTabControl.Items.Add(tabItem);
            }
        }

        private Grid BuildTabContent(GroupTab tab)
        {
            var dataGrid = new DataGrid
            {
                ItemsSource = tab.Rows,
                AutoGenerateColumns = false,
                IsReadOnly = true,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                RowHeaderWidth = 0,
            };

            dataGrid.SetResourceReference(BackgroundProperty, "MaterialDesignPaper");
            dataGrid.SetResourceReference(BorderBrushProperty, "MaterialDesign.Brush.Foreground");
            dataGrid.BorderThickness = new Thickness(1);

            if (TryFindResource("NotoSansKhmer") is FontFamily notoSansKhmer)
            {
                var columnHeaderStyle = new Style(typeof(DataGridColumnHeader));
                columnHeaderStyle.Setters.Add(new Setter(FontWeightProperty, FontWeights.Bold));
                columnHeaderStyle.Setters.Add(new Setter(PaddingProperty, new Thickness(10)));
                columnHeaderStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                columnHeaderStyle.Setters.Add(new Setter(TextBlock.FontFamilyProperty, notoSansKhmer));
                dataGrid.ColumnHeaderStyle = columnHeaderStyle;

                var cellStyle = new Style(typeof(DataGridCell));
                cellStyle.Setters.Add(new Setter(TextBlock.FontFamilyProperty, notoSansKhmer));
                cellStyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, 16.0));
                dataGrid.CellStyle = cellStyle;
            }

            var cellTextStyle = TryFindResource("CellTextStyle") as Style;
            GenerateColumns(dataGrid, tab, cellTextStyle);

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetRow(dataGrid, 0);
            grid.Children.Add(dataGrid);

            SummaryContent summaryContent = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Summary = BuildSummaryText(tab.Summary)
            };
            Grid.SetRow(summaryContent, 1);
            grid.Children.Add(summaryContent);

            return grid;
        }

        private static void GenerateColumns(DataGrid dataGrid, GroupTab tab, Style? cellTextBaseStyle)
        {
            dataGrid.Columns.Clear();

            foreach (var col in tab.Columns)
            {
                bool isImageColumn = tab.Rows.Count > 0
                    && tab.Rows[0].Values.TryGetValue(col.Key, out var firstCell)
                    && firstCell.Value is byte[];

                if (isImageColumn)
                {
                    var binding = new Binding($"Values[{col.Key}].Value")
                    {
                        Converter = _bytesToImageConverter,
                    };

                    var factory = new FrameworkElementFactory(typeof(Image));
                    factory.SetBinding(Image.SourceProperty, binding);
                    factory.SetValue(WidthProperty, 60.0);
                    factory.SetValue(HeightProperty, 80.0);
                    factory.SetValue(Image.StretchProperty, Stretch.Uniform);
                    factory.SetValue(MarginProperty, new Thickness(4));

                    dataGrid.Columns.Add(new DataGridTemplateColumn
                    {
                        Header = col.DisplayName,
                        Width = col.Width > 0 ? new DataGridLength(col.Width) : DataGridLength.Auto,
                        CellTemplate = new DataTemplate { VisualTree = factory },
                    });
                }
                else
                {
                    var binding = new Binding($"Values[{col.Key}]");

                    var textCol = new DataGridTextColumn
                    {
                        Header = col.DisplayName,
                        Binding = binding,
                        Width = col.Width > 0 ? new DataGridLength(col.Width) : DataGridLength.Auto,
                    };

                    var style = new Style(typeof(TextBlock), cellTextBaseStyle);

                    if (col.FontSize.HasValue)
                        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, col.FontSize.Value));

                    if (col.IsBold)
                        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));

                    if (col.ForegroundColor != null)
                        style.Setters.Add(new Setter(TextBlock.ForegroundProperty, col.ForegroundColor));

                    if (col.Alignment != CellAlignment.Left)
                        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty,
                            col.Alignment == CellAlignment.Center ? System.Windows.TextAlignment.Center : System.Windows.TextAlignment.Right));

                    textCol.ElementStyle = style;

                    dataGrid.Columns.Add(textCol);
                }
            }
        }

        private static string BuildSummaryText(Dictionary<string, object?> summary)
        {
            if (summary?.Count > 0)
            {
                return string.Join(" | ", summary
                    .Where(kvp => !kvp.Key.StartsWith("__"))
                    .Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            }

            return string.Empty;
        }
    }
}
