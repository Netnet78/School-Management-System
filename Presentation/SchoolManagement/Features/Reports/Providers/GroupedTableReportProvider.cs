using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Views;

namespace SchoolManagement.Presentation.Features.Reports.Providers
{
    public partial class GroupedTableReportProvider : ObservableObject, IReportViewProvider, IGroupedReportTablePreviewProvider
    {
        public string ReportTypeKey { get; }

        public IReportFilterViewModel? FilterViewModel { get; }

        public IReportOptionsViewModel? OptionsViewModel => null;

        public string[] SupportedExportFormats => ["Excel"];

        private readonly IReportGenerator _generator;

        public UserControl PreviewView { get; }

        [ObservableProperty]
        private GroupedReportTableData? _groupedTableData;

        [ObservableProperty]
        private bool _hasData;

        [ObservableProperty]
        private string _summaryText = string.Empty;

        [ObservableProperty]
        private int _totalCount;

        private object? _lastFilter;
        private int _generationId;

        public GroupedTableReportProvider(
            string reportTypeKey,
            IReportGenerator generator,
            IReportFilterViewModel? filterVm)
        {
            ReportTypeKey = reportTypeKey;
            _generator = generator;
            FilterViewModel = filterVm;
            PreviewView = new GroupedReportTablePreviewView { DataContext = this };
        }

        public async Task GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            int generationId = Interlocked.Increment(ref _generationId);

            var result = await _generator.GenerateAsync(filter, cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                return;

            string summaryText = BuildSummaryText(result);

            if (result is GroupedTableReportResult groupedResult)
            {
                var groupedData = await Task.Run(
                    () => RenderGroupedData(groupedResult),
                    cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                    return;

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                        return;

                    GroupedTableData = groupedData;
                    HasData = groupedData.HasData;
                    SummaryText = summaryText;
                    TotalCount = groupedResult.Groups.Sum(g => g.Rows.Count);
                    _lastFilter = filter;
                });
            }
            else
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                        return;

                    GroupedTableData = null;
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
                var result = await _generator.GenerateAsync(_lastFilter, cancellationToken).ConfigureAwait(false);
                await exporter.ExportToFileAsync(result, filePath, cancellationToken).ConfigureAwait(false);
            }
        }

        private static GroupedReportTableData RenderGroupedData(GroupedTableReportResult result)
        {
            var data = new GroupedReportTableData();

            foreach (var group in result.Groups)
            {
                var columns = group.Columns.Select(c => new ReportTableColumnInfo
                {
                    Key = c.Key,
                    DisplayName = c.HeaderKhmer ?? c.Header,
                    Width = c.Width,
                    FontSize = c.FontSize,
                    IsBold = c.IsBold,
                    Alignment = c.Alignment,
                    ForegroundColor = c.ForegroundColor,
                    BackgroundColor = c.BackgroundColor,
                }).ToList();

                var rows = new ObservableCollection<ReportTableRow>(
                    group.Rows.Select(r => new ReportTableRow
                    {
                        Values = r.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    }));

                data.Tabs.Add(new GroupTab
                {
                    Header = group.KhmerName ?? group.Name,
                    Columns = columns,
                    Rows = rows,
                    Summary = result.Summary
                });
            }

            return data;
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
