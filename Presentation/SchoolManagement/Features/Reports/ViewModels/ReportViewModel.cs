using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.ViewModels
{
    public partial class ReportViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IReportRegistry _registry;
        private readonly IEnumerable<IReportViewProvider> _providers;
        private readonly IEnumerable<IReportExporter> _exporters;
        private readonly IMessageService _messageService;
        private readonly ILoadingService _loadingService;

        public IEnumerable<IReportExporter> Exporters =>
            _currentProvider != null
                ? _exporters.Where(e => _currentProvider.CanExport(e))
                : [];

        private IReportViewProvider? _currentProvider;
        private CancellationTokenSource? _previewGenerationCts;
        private int _previewGenerationId;
        private int _loadingOperationCount;

        public IEnumerable<ReportCardItem> VisibleReportCards => ReportCards;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VisibleReportCards))]
        private ObservableCollection<ReportCardItem> _reportCards = [];

        [ObservableProperty]
        private ReportCardItem? _selectedCard;

        [ObservableProperty]
        private IReportFilterViewModel? _currentFilterViewModel;

        [ObservableProperty]
        private UserControl? _currentPreviewView;

        [ObservableProperty]
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
        [NotifyPropertyChangedFor(nameof(MaxPage))]
        [NotifyPropertyChangedFor(nameof(PageInfo))]
        private int _totalCount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MaxPage))]
        [NotifyPropertyChangedFor(nameof(PageInfo))]
        private int _currentPage = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MaxPage))]
        [NotifyPropertyChangedFor(nameof(PageInfo))]
        private int _pageSize = 10;

        [ObservableProperty]
        private bool _showPageOption = false;

        [ObservableProperty]
        public bool _hasSummary;

        public int MaxPage => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;

        public string PageInfo => $"Page {CurrentPage} of {MaxPage} ({TotalCount} items)";

        public List<int> AvailablePageSizes { get; } = [10, 20, 50, 100];

        public bool ShowNoDataMessage => !HasData && !IsLoading;

        public ReportViewModel(
            IReportRegistry registry,
            IEnumerable<IReportViewProvider> providers,
            IEnumerable<IReportExporter> exporters,
            IMessageService messageService,
            ILoadingService loadingService)
        {
            _registry = registry;
            _providers = providers;
            _exporters = exporters;
            _messageService = messageService;
            _loadingService = loadingService;
        }

        public async Task LoadAsync()
        {
            BeginLoadingOperation();

            try
            {
                var definitions = _registry.GetAllDescriptors();
                var cards = definitions.Select(d => new ReportCardItem(d.Definition)).ToList();
                ReportCards = new ObservableCollection<ReportCardItem>(cards);

                if (cards.Count > 0)
                {
                    cards[0].IsSelected = true;
                    await SelectReportAsync(cards[0]);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"Error loading reports: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                EndLoadingOperation();
            }
        }

        [RelayCommand]
        private async Task SelectReportAsync(ReportCardItem card)
        {
            BeginLoadingOperation();

            try
            {
                if (SelectedCard != null)
                    SelectedCard.IsSelected = false;

                SelectedCard = card;
                card.IsSelected = true;
                SelectedReportTitle = card.DisplayName;

                // Find provider by key
                IReportViewProvider? provider = _providers.FirstOrDefault(p => p.ReportTypeKey == card.Key);
                if (provider == null)
                {
                    _messageService.Show($"No provider found for report '{card.Key}'", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                // Clean up previous provider
                if (CurrentFilterViewModel != null)
                {
                    CurrentFilterViewModel.FilterChanged -= OnFilterChanged;
                }

                _currentProvider = provider;
                OnPropertyChanged(nameof(Exporters));
                HasData = false;
                SummaryText = string.Empty;
                HasSummary = string.IsNullOrWhiteSpace(_currentProvider.SummaryText) && HasData;
                CurrentPreviewView = null;

                // Wire filter
                if (provider.FilterViewModel is { } filterVm)
                {
                    filterVm.FilterChanged += OnFilterChanged;
                    CurrentFilterViewModel = filterVm;
                    IsFilterVisible = true;

                    if (filterVm is IAsyncLoadable asyncLoadable)
                    {
                        await asyncLoadable.LoadAsync();
                    }
                }
                else
                {
                    CurrentFilterViewModel = null;
                    IsFilterVisible = false;
                }

                CurrentPreviewView = provider.PreviewView;

                CurrentPage = 1;

                // Trigger initial generation
                var filter = provider.FilterViewModel?.GetFilterData() ?? new object();
                await GenerateWithProviderAsync(filter);
            }
            catch (Exception ex)
            {
                _messageService.Show($"Error selecting report: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                EndLoadingOperation();
            }
        }

        [RelayCommand]
        private async Task ClearFilters()
        {
            CurrentFilterViewModel?.ResetFilterData();
        }

        private async Task OnFilterChangedAsync()
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => { });

            if (_currentProvider == null || CurrentFilterViewModel == null)
                return;

            CurrentPage = 1;
            await GenerateWithProviderAsync(CurrentFilterViewModel.GetFilterData());
        }

        private async void OnFilterChanged()
        {
            await OnFilterChangedAsync();
        }

        private async Task GenerateWithProviderAsync(object filter)
        {
            IReportViewProvider? provider = _currentProvider;
            if (provider == null)
                return;

            if (filter is IPagedFilter paged)
            {
                ShowPageOption = true;
                paged.Page = CurrentPage;
                paged.PageSize = PageSize;
            }
            else
            {
                ShowPageOption = false;
            }

            (int generationId, CancellationToken cancellationToken) = BeginPreviewGeneration();
            await Task.Yield();

            try
            {
                await _loadingService.ShowLoading("កំពុងដំណើរការ...");

                await provider.GenerateAsync(filter, cancellationToken);

                if (cancellationToken.IsCancellationRequested || !IsCurrentPreviewGeneration(generationId))
                    return;

                HasData = provider.HasData;
                SummaryText = provider.SummaryText;
                HasSummary = !string.IsNullOrWhiteSpace(SummaryText) && HasData;
                TotalCount = provider.TotalCount;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested || !IsCurrentPreviewGeneration(generationId))
                    return;

                _messageService.Show($"Error generating report: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                await _loadingService.Hide();
                EndLoadingOperation();
            }
        }

        [RelayCommand]
        private async Task ExportAsync(IReportExporter exporter)
        {
            var provider = _currentProvider;
            if (provider == null)
                return;

            string extension = exporter.FileExtension;
            SaveFileDialog saveDialog = new()
            {
                Title = "Save Report",
                Filter = $"{exporter.FormatName} files (*{extension})|*{extension}",
                FileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}{extension}",
            };

            if (saveDialog.ShowDialog() != true)
                return;

            string filePath = saveDialog.FileName;

            BeginLoadingOperation();
            await Task.Yield();

            try
            {
                await _loadingService.ShowLoading("កំពុងដំណើរការ...");

                await provider.ExportAsync(exporter, filePath);

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
                await _loadingService.Hide();
                EndLoadingOperation();
            }
        }

        private (int GenerationId, CancellationToken Token) BeginPreviewGeneration()
        {
            var nextCts = new CancellationTokenSource();
            var previousCts = Interlocked.Exchange(ref _previewGenerationCts, nextCts);
            previousCts?.Cancel();
            previousCts?.Dispose();

            int generationId = Interlocked.Increment(ref _previewGenerationId);
            BeginLoadingOperation();
            return (generationId, nextCts.Token);
        }

        private static bool IsCurrentPreviewGeneration(int generationId, int currentGenerationId)
            => generationId == currentGenerationId;

        private bool IsCurrentPreviewGeneration(int generationId)
            => IsCurrentPreviewGeneration(generationId, Volatile.Read(ref _previewGenerationId));

        [RelayCommand]
        private async Task GoToNextPage()
        {
            if (CurrentPage < MaxPage)
            {
                CurrentPage++;
                await RegenerateAsync();
            }
        }

        [RelayCommand]
        private async Task GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await RegenerateAsync();
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            _ = OnPageSizeChangedAsync();
        }

        private async Task OnPageSizeChangedAsync()
        {
            CurrentPage = 1;
            await RegenerateAsync();
        }

        private async Task RegenerateAsync()
        {
            if (_currentProvider == null)
                return;

            var filter = CurrentFilterViewModel?.GetFilterData() ?? new object();
            await GenerateWithProviderAsync(filter);
        }

        private void BeginLoadingOperation()
        {
            if (Interlocked.Increment(ref _loadingOperationCount) == 1)
            {
                IsLoading = true;
            }
        }

        private void EndLoadingOperation()
        {
            if (Interlocked.Decrement(ref _loadingOperationCount) == 0)
            {
                IsLoading = false;
            }
        }
    }
}
