using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Views;

namespace SchoolManagement.Presentation.Features.Reports.Providers
{
    public abstract partial class TableReportProvider : ObservableObject, IReportViewProvider, IReportTablePreviewProvider
    {
        public abstract string ReportTypeKey { get; }

        public abstract IReportFilterViewModel? FilterViewModel { get; }

        public IReportOptionsViewModel? OptionsViewModel => null;

        public abstract string[] SupportedExportFormats { get; }

        protected abstract Task<ReportResult> GenerateReportAsync(object filter, CancellationToken cancellationToken);

        private readonly IReportRenderer _renderer;

        public UserControl PreviewView { get; }

        [ObservableProperty]
        private ReportTableData? _tableData;

        [ObservableProperty]
        private bool _hasData;

        [ObservableProperty]
        private string _summaryText = string.Empty;

        [ObservableProperty]
        private int _totalCount;

        private object? _lastFilter;
        private int _generationId;
        private readonly SemaphoreSlim _generationLock = new(1, 1);

        protected TableReportProvider(IReportRenderer renderer)
        {
            _renderer = renderer;
            PreviewView = new ReportTablePreviewView { DataContext = this };
        }

        public async Task GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            int generationId = Interlocked.Increment(ref _generationId);

            ReportResult result;
            await _generationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = await GenerateReportAsync(filter, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _generationLock.Release();
            }

            if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                return;

            string summaryText = BuildSummaryText(result);

            if (result is TableReportResult && _renderer.CanRender(result))
            {
                ReportTableData tableData = await Task.Run(
                    async() => (ReportTableData)await _renderer.Render(result),
                    cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                    return;

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                        return;

                    TableData = tableData;
                    HasData = TableData.HasData;
                    SummaryText = summaryText;
                    TotalCount = result.Summary?.TryGetValue("__totalCount", out var tc) == true && tc is int total
                        ? total
                        : tableData.Rows.Count;
                    _lastFilter = filter;
                });
            }
            else
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                        return;

                    TableData = null;
                    HasData = false;
                    SummaryText = summaryText;
                    _lastFilter = filter;
                });
            }
        }

        public async Task ExportAsync(IReportExporter exporter, string filePath, CancellationToken cancellationToken = default)
        {
            if (!SupportedExportFormats.Contains(exporter.FormatName))
                throw new InvalidOperationException($"Table reports do not support {exporter.FormatName} export.");

            if (_lastFilter != null)
            {
                var result = await GenerateReportAsync(_lastFilter, cancellationToken).ConfigureAwait(false);
                await exporter.ExportToFileAsync(result, filePath, cancellationToken).ConfigureAwait(false);
            }
        }

        private static string BuildSummaryText(ReportResult result)
        {
            if (result.Summary?.Count > 0)
            {
                return string.Join(" | ", result.Summary
                    .Where(kvp => !kvp.Key.StartsWith("__"))
                    .Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            }

            return string.Empty;
        }
    }
}
