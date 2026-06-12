using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Converters;
using SchoolManagement.Presentation.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Providers;

namespace SchoolManagement.Presentation.Features.Reports.Views
{
    public partial class ReportTablePreviewView : UserControl
    {
        private static readonly BytesToImageConverter _bytesToImageConverter = new();
        private IReportTablePreviewProvider? _previewProvider;
        private CheckBox? _headerCheckBox;

        public ReportTablePreviewView()
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

            if (e.NewValue is not IReportTablePreviewProvider provider)
            {
                GenerateColumns(null);
                return;
            }

            _previewProvider = provider;
            _previewProvider.PropertyChanged += OnPreviewProviderPropertyChanged;
            GenerateColumns(provider.TableData);
        }

        private void OnPreviewProviderPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not IReportTablePreviewProvider provider)
                return;

            if (e.PropertyName == nameof(IReportTablePreviewProvider.TableData))
            {
                GenerateColumns(provider.TableData);
            }
            else if (e.PropertyName == "IsAllSelected" && _headerCheckBox != null && provider is StudentCardTableReportProvider cardProvider)
            {
                _headerCheckBox.IsChecked = cardProvider.IsAllSelected;
            }
        }

        private void GenerateColumns(ReportTableData? data)
        {
            ReportDataGrid.Columns.Clear();
            _headerCheckBox = null;

            if (data?.Columns == null)
                return;

            if (data.HasSelection)
            {
                _headerCheckBox = new CheckBox
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                if (_previewProvider is StudentCardTableReportProvider cardProvider)
                {
                    _headerCheckBox.IsChecked = cardProvider.IsAllSelected;
                    _headerCheckBox.Click += OnHeaderCheckBoxClick;
                }

                var cellFactory = new FrameworkElementFactory(typeof(CheckBox));
                cellFactory.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsSelected")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                });
                cellFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                cellFactory.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);

                cellFactory.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(OnRowCheckBoxChecked));
                cellFactory.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(OnRowCheckBoxUnchecked));

                ReportDataGrid.Columns.Add(new DataGridTemplateColumn
                {
                    Header = _headerCheckBox,
                    Width = 50,
                    CellTemplate = new DataTemplate { VisualTree = cellFactory },
                });
            }

            foreach (var col in data.Columns)
            {
                bool isImageColumn = data.Rows.Count > 0
                    && data.Rows[0].Values.TryGetValue(col.Key, out var firstCell)
                    && firstCell.Value is byte[];

                if (isImageColumn)
                {
                    var binding = new Binding($"Values[{col.Key}].Value")
                    {
                        Converter = _bytesToImageConverter,
                    };

                    var factory = new FrameworkElementFactory(typeof(Image));
                    factory.SetBinding(Image.SourceProperty, binding);
                    factory.SetValue(Image.WidthProperty, 60.0);
                    factory.SetValue(Image.HeightProperty, 80.0);
                    factory.SetValue(Image.StretchProperty, Stretch.Uniform);
                    factory.SetValue(FrameworkElement.MarginProperty, new Thickness(4));

                    ReportDataGrid.Columns.Add(new DataGridTemplateColumn
                    {
                        Header = col.DisplayName,
                        Width = col.Width > 0 ? new DataGridLength(col.Width) : DataGridLength.Auto,
                        CellTemplate = new DataTemplate { VisualTree = factory },
                    });
                }
                else
                {
                    var binding = new Binding($"Values[{col.Key}]")
                    {
                        StringFormat = string.IsNullOrEmpty(col.DisplayName) ? null : null,
                    };

                    var textCol = new DataGridTextColumn
                    {
                        Header = col.DisplayName,
                        Binding = binding,
                        Width = col.Width > 0 ? new DataGridLength(col.Width) : DataGridLength.Auto,
                    };

                    var style = new Style(typeof(TextBlock), (Style)FindResource("CellTextStyle"));

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

                    ReportDataGrid.Columns.Add(textCol);
                }
            }
        }

        private void OnHeaderCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox && _previewProvider is StudentCardTableReportProvider cardProvider)
            {
                cardProvider.ToggleSelectAll();
            }
        }

        private void OnRowCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox && _previewProvider is StudentCardTableReportProvider cardProvider)
            {
                int? id = ExtractStudentId(e.Source as CheckBox);
                if (id.HasValue)
                {
                    cardProvider.SelectStudent(id.Value);
                }
            }
        }

        private void OnRowCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox && _previewProvider is StudentCardTableReportProvider cardProvider)
            {
                int? id = ExtractStudentId(e.Source as CheckBox);
                if (id.HasValue)
                {
                    cardProvider.DeselectStudent(id.Value);
                }
            }
        }

        private static int? ExtractStudentId(CheckBox? checkBox)
        {
            if (checkBox?.DataContext is ReportTableRow row
                && row.Values.TryGetValue("__studentRawId", out var cell)
                && cell.Value is int id
                && id > 0)
                return id;
            return null;
        }
    }
}
