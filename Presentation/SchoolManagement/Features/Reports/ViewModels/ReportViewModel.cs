using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Models;
using SchoolManagement.Presentation.Shared.Converters;

namespace SchoolManagement.Presentation.Features.Reports.ViewModels
{
    public partial class ReportViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IReportRegistry _registry;
        private readonly IReportComponentFactory _componentFactory;
        private readonly IMessageService _messageService;
        private readonly IEnumerable<IReportExporter> _exporters;

        public IEnumerable<IReportExporter> Exporters => _exporters;

        private IReportGenerator? _currentGenerator;

        [ObservableProperty]
        private ObservableCollection<ReportCardItem> _reportCards = [];

        [ObservableProperty]
        private ReportCardItem? _selectedCard;

        [ObservableProperty]
        private IReportFilterViewModel? _currentFilterViewModel;

        [ObservableProperty]
        private DataTable? _previewTable;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSummary))]
        [NotifyPropertyChangedFor(nameof(ShowNoDataMessage))]
        private bool _hasData;

        [ObservableProperty]
        private bool _isFilterVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowNoDataMessage))]
        private bool _isLoading;

        [ObservableProperty]
        private string _summaryText = string.Empty;

        [ObservableProperty]
        private string _selectedReportTitle = string.Empty;

        [ObservableProperty]
        private ObservableCollection<CardPreviewGroup>? _cardPreviewGroups;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSummary))]
        private bool _hasCardPreview;

        public bool HasSummary => HasData || HasCardPreview;

        public bool ShowNoDataMessage => !HasData && !HasCardPreview && !IsLoading;

        public ReportViewModel(
            IReportRegistry registry,
            IReportComponentFactory componentFactory,
            IMessageService messageService,
            IEnumerable<IReportExporter> exporters)
        {
            _registry = registry;
            _componentFactory = componentFactory;
            _messageService = messageService;
            _exporters = exporters;
        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            var definitions = _registry.GetAll();
            var cards = definitions.Select(d => new ReportCardItem(d)).ToList();
            ReportCards = new ObservableCollection<ReportCardItem>(cards);

            if (cards.Count > 0)
            {
                cards[0].IsSelected = true;
                await SelectReportAsync(cards[0]);
            }

            IsLoading = false;
        }

        [RelayCommand]
        private async Task SelectReportAsync(ReportCardItem card)
        {
            try
            {
                if (SelectedCard != null)
                    SelectedCard.IsSelected = false;

                SelectedCard = card;
                card.IsSelected = true;
                SelectedReportTitle = card.DisplayName;

                var definition = card.Definition;

                // Create generator instance via factory
                _currentGenerator = _componentFactory.CreateGenerator(definition);

                // Create and load filter VM
                CurrentFilterViewModel = null;
                IsFilterVisible = false;
                HasData = false;
                HasCardPreview = false;
                PreviewTable = null;
                CardPreviewGroups = null;

                var filterVm = _componentFactory.CreateFilterViewModel(definition);
                filterVm.FilterChanged += OnFilterChanged;
                CurrentFilterViewModel = filterVm;
                IsFilterVisible = true;

                if (filterVm is IAsyncLoadable asyncLoadable)
                {
                    await asyncLoadable.LoadAsync();
                }

                // Trigger initial preview
                await GeneratePreviewAsync();
            }
            catch (Exception ex)
            {
                _messageService.Show($"Error selecting report: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
                IsLoading = false;
            }
        }

        private async void OnFilterChanged()
        {
            await GeneratePreviewAsync();
        }

        private async Task GeneratePreviewAsync()
        {
            if (_currentGenerator == null || CurrentFilterViewModel == null)
                return;

            IsLoading = true;

            try
            {
                var filterData = CurrentFilterViewModel.GetFilterData();
                var result = await _currentGenerator.GenerateAsync(filterData);

                if (result.Layout == ReportLayout.Card && result.CardGroups is { Count: > 0 })
                {
                    // Build card preview
                    var groups = new ObservableCollection<CardPreviewGroup>();

                    foreach (var cardGroup in result.CardGroups)
                    {
                        var group = new CardPreviewGroup
                        {
                            Width = cardGroup.Width,
                            Height = cardGroup.Height,
                        };

                        foreach (var item in cardGroup.Items)
                        {
                            var previewItem = new CardPreviewItem
                            {
                                X = item.XPos,
                                Y = item.YPos,
                                Text = item.Value as string,
                                ImageBytes = item.Value as byte[],
                            };

                            if (previewItem.ImageBytes != null)
                            {
                                previewItem.ImageSource = previewItem.ImageBytes.ConvertToBitmapsource();
                            }

                            group.Items.Add(previewItem);
                        }

                        groups.Add(group);
                    }

                    CardPreviewGroups = groups;
                    HasCardPreview = groups.Count > 0;
                    HasData = false;
                    PreviewTable = null;
                }
                else
                {
                    // Build DataTable preview (table layout)
                    var table = new DataTable();

                    foreach (var col in result.Columns)
                    {
                        var header = !string.IsNullOrEmpty(col.HeaderKhmer) ? col.HeaderKhmer : col.Header;
                        table.Columns.Add(col.Key, typeof(string));
                    }

                    foreach (var row in result.Rows)
                    {
                        var dr = table.NewRow();
                        foreach (var col in result.Columns)
                        {
                            dr[col.Key] = row.GetValueOrDefault(col.Key)?.ToString() ?? "";
                        }
                        table.Rows.Add(dr);
                    }

                    PreviewTable = table;
                    HasData = table.Rows.Count > 0;
                    HasCardPreview = false;
                    CardPreviewGroups = null;
                }

                // Build summary text
                var summaryParts = new List<string>();
                if (result.Summary != null)
                {
                    foreach (var kvp in result.Summary)
                    {
                        summaryParts.Add($"{kvp.Key}: {kvp.Value}");
                    }
                }
                SummaryText = string.Join(" | ", summaryParts);
            }
            catch (Exception ex)
            {
                _messageService.Show($"Error generating report: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportAsync(IReportExporter exporter)
        {
            if (_currentGenerator == null || CurrentFilterViewModel == null)
                return;

            IsLoading = true;

            try
            {
                var filterData = CurrentFilterViewModel.GetFilterData();
                var result = await _currentGenerator.GenerateAsync(filterData);

                string extension = exporter.FileExtension;
                var saveDialog = new SaveFileDialog
                {
                    Title = "Save Report",
                    Filter = $"{exporter.FormatName} files (*{extension})|*{extension}",
                    FileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}{extension}",
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                string filePath = saveDialog.FileName;

                await exporter.ExportToFileAsync(result, filePath);

                var openResult = _messageService.Show(
                    "Report generated successfully! Do you want to open the file?",
                    "Success",
                    MessageButton.YesNo,
                    MessageIcon.Information);

                if (openResult == MessageResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"Error exporting report: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
