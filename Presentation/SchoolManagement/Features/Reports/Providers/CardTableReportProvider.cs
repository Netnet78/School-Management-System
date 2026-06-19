using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Generators;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentCard;
using SchoolManagement.Presentation.Features.Reports.Views;

namespace SchoolManagement.Presentation.Features.Reports.Providers;

public partial class CardTableReportProvider : ObservableObject, IReportViewProvider, IReportTablePreviewProvider, ISelectableItemReport
{
    public string ReportTypeKey { get; }

    public IReportFilterViewModel? FilterViewModel { get; }

    public string[] SupportedExportFormats => ["PDF"];

    private readonly IReportGenerator _generator;

    public IReportOptionsViewModel? OptionsViewModel { get; }

    public UserControl PreviewView { get; }

    [ObservableProperty]
    private ReportTableData? _tableData;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _summaryText = string.Empty;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isAllSelected;

    [ObservableProperty]
    private bool _showSelectedOnly;

    private ReportResult? _lastResult;
    private object? _lastFilter;
    private object? _originalFilter;
    private int _generationId;

    private readonly HashSet<int> _selectedIds = [];
    private readonly object _selectionLock = new();
    private bool _isBatchUpdating;
    private bool _optionsChangedSinceLastGeneration;

    public CardTableReportProvider(
        string reportTypeKey,
        IReportGenerator generator,
        IReportFilterViewModel? filterVm,
        IReportOptionsViewModel? optionsVm = null)
    {
        ReportTypeKey = reportTypeKey;
        _generator = generator;
        FilterViewModel = filterVm;
        OptionsViewModel = optionsVm;
        PreviewView = new ReportTablePreviewView { DataContext = this };

        if (optionsVm != null)
        {
            optionsVm.OptionsChanged += OnOptionsChanged;
        }
    }

    public void SelectItem(int id)
    {
        if (!_isBatchUpdating)
        {
            lock (_selectionLock)
            {
                _selectedIds.Add(id);
            }

            UpdateIsAllSelected();
        }
    }

    public void DeselectItem(int id)
    {
        if (!_isBatchUpdating)
        {
            lock (_selectionLock)
            {
                _selectedIds.Remove(id);
            }

            UpdateIsAllSelected();
        }
    }

    public void ToggleSelectAll()
    {
        if (TableData == null) return;

        bool newState = !IsAllSelected;
        _isBatchUpdating = true;

        if (newState)
        {
            foreach (var row in TableData.Rows)
            {
                int? entryId = ExtractRowId(row);
                if (entryId.HasValue)
                {
                    lock (_selectionLock)
                    {
                        _selectedIds.Add(entryId.Value);
                    }
                }
                row.IsSelected = true;
            }
        }
        else
        {
            foreach (var row in TableData.Rows)
            {
                int? entryId = ExtractRowId(row);
                if (entryId.HasValue)
                {
                    lock (_selectionLock)
                    {
                        _selectedIds.Remove(entryId.Value);
                    }
                }
                row.IsSelected = false;
            }
        }

        _isBatchUpdating = false;
        IsAllSelected = newState;
    }

    private void UpdateIsAllSelected()
    {
        if (TableData != null)
        {
            int total = TableData.Rows.Count;
            int selected = TableData.Rows.Count(r => r.IsSelected);
            IsAllSelected = total > 0 && selected == total;
        }
    }

    [RelayCommand]
    private void ClearAllSelections()
    {
        lock (_selectionLock)
        {
            _selectedIds.Clear();
        }

        if (TableData != null)
        {
            foreach (var row in TableData.Rows)
            {
                row.IsSelected = false;
            }
        }
        IsAllSelected = false;

        if (ShowSelectedOnly)
        {
            ShowSelectedOnly = false;
        }
    }

    public async Task GenerateAsync(object filter, CancellationToken cancellationToken = default)
    {
        ShowSelectedOnly = false;
        _originalFilter = filter;

        await GenerateInternalAsync(filter, cancellationToken);
    }

    private async Task GenerateInternalAsync(object filter, CancellationToken cancellationToken = default)
    {
        int generationId = Interlocked.Increment(ref _generationId);
        HashSet<int> selectedIdsSnapshot = GetSelectedIdSnapshot();

        object request = filter;
        if (OptionsViewModel != null)
        {
            request = MergeOptionsIntoFilter(filter);
            _optionsChangedSinceLastGeneration = false;
        }

        var result = await _generator.GenerateAsync(request, cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
            return;

        _lastResult = result;
        _lastFilter = filter;

        if (result is CardReportResult cardResult)
        {
            var tableData = await Task.Run(
                () => ConvertToTableData(cardResult, selectedIdsSnapshot),
                cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                return;

            string summaryText = BuildSummaryText(result);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                    return;

                tableData.HasSelection = true;
                int fullCount = result.Summary?.TryGetValue("__totalCount", out var tc) == true && tc is int totalCount
                    ? totalCount
                    : tableData.Rows.Count;
                TotalCount = fullCount;
                HasData = tableData.HasData;
                SummaryText = summaryText;
                TableData = tableData;
                IsAllSelected = tableData.Rows.Count > 0 && tableData.Rows.All(r => r.IsSelected);
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
                SummaryText = BuildSummaryText(result);
                _lastFilter = filter;
            });
        }
    }

    private void OnOptionsChanged()
    {
        _optionsChangedSinceLastGeneration = true;
    }

    partial void OnShowSelectedOnlyChanged(bool value)
    {
        if (_originalFilter == null) return;

        if (value)
        {
            HashSet<int> selectedIds = GetSelectedIdSnapshot();
            if (selectedIds.Count == 0)
            {
                ShowSelectedOnly = false;
                return;
            }

            var selectFilter = new StudentCardFilter
            {
                ManualSelectEnabled = true,
                SelectedStudentIds = selectedIds.ToList(),
                PageSize = null,
            };
            _ = GenerateInternalAsync(selectFilter);
        }
        else
        {
            _ = GenerateInternalAsync(_originalFilter);
        }
    }

    [RelayCommand]
    private async Task SelectAllFilteredAsync()
    {
        if (_generator is not StudentCardGenerator cardGenerator || _lastFilter is not StudentCardFilter filter)
            return;

        var ids = await cardGenerator.GetMatchingStudentIdsAsync(filter).ConfigureAwait(false);

        _isBatchUpdating = true;
        lock (_selectionLock)
        {
            _selectedIds.Clear();
            foreach (var id in ids)
            {
                _selectedIds.Add(id);
            }
        }
        _isBatchUpdating = false;

        if (TableData != null)
        {
            HashSet<int> selectedIds = GetSelectedIdSnapshot();
            foreach (var row in TableData.Rows)
            {
                int? entryId = ExtractRowId(row);
                if (entryId.HasValue)
                {
                    row.IsSelected = selectedIds.Contains(entryId.Value);
                }
            }
            UpdateIsAllSelected();
        }

        if (ShowSelectedOnly)
        {
            HashSet<int> selectedIds = GetSelectedIdSnapshot();
            var selectFilter = new StudentCardFilter
            {
                ManualSelectEnabled = true,
                SelectedStudentIds = selectedIds.ToList(),
            };
            await GenerateInternalAsync(selectFilter);
        }
    }

    public async Task ExportAsync(IReportExporter exporter, string filePath, CancellationToken cancellationToken = default)
    {
        if (!SupportedExportFormats.Contains(exporter.FormatName))
            throw new InvalidOperationException($"Report does not support {exporter.FormatName} export.");

        HashSet<int> selectedIdsSnapshot = GetSelectedIdSnapshot();
        if (selectedIdsSnapshot.Count == 0)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show("No items selected for export. Please select items first using checkboxes or \"Select All Filtered\".",
                    "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
            return;
        }

        if (_optionsChangedSinceLastGeneration)
        {
            var selectFilter = new StudentCardFilter
            {
                ManualSelectEnabled = true,
                SelectedStudentIds = selectedIdsSnapshot.ToList(),
            };
            var request = MergeOptionsIntoFilter(selectFilter);
            _lastResult = await _generator.GenerateAsync(request, cancellationToken).ConfigureAwait(false);
            _optionsChangedSinceLastGeneration = false;
        }

        if (_lastResult == null) return;

        await exporter.ExportToFileAsync(_lastResult, filePath, cancellationToken).ConfigureAwait(false);
    }
    private StudentCardReportRequest MergeOptionsIntoFilter(object filter)
    {
        StudentCardFilter cardFilter = (StudentCardFilter)filter;

        var options = new StudentCardOptions();
        if (OptionsViewModel is StudentCardOptionsViewModel optionsVm)
        {
            options.PrincipalName = optionsVm.PrincipalName;
            options.SignaturePath = optionsVm.SignaturePath;
            options.Location = optionsVm.Location;
            options.ReportDate = optionsVm.CreatedDate;
        }

        return new StudentCardReportRequest
        {
            Filter = cardFilter,
            Options = options,
        };
    }

    private ReportTableData ConvertToTableData(CardReportResult cardResult, HashSet<int> selectedIds)
    {
        var data = new ReportTableData();

        if (cardResult.Columns != null)
        {
            foreach (var col in cardResult.Columns)
            {
                data.Columns.Add(new ReportTableColumnInfo
                {
                    Key = col.Key,
                    DisplayName = col.HeaderKhmer ?? col.Header,
                    Width = col.Width,
                });
            }
        }

        if (cardResult.CardGroups == null)
            return data;

        foreach (var group in cardResult.CardGroups)
        {
            var row = new ReportTableRow();
            var fieldMap = group.Items
                .Where(i => i.FieldName != null)
                .ToDictionary(i => i.FieldName!, i => i);

            foreach (var col in data.Columns)
            {
                if (col.Key == "Photo")
                {
                    row.Values[col.Key] = new ReportCell
                    {
                        Value = fieldMap.GetValueOrDefault("Photo")?.Value is BitmapInfo bmp ? bmp.Data : null
                    };
                    continue;
                }

                if (col.Key == "__rawId")
                    continue;

                var cardItem = fieldMap.GetValueOrDefault(col.Key);
                row.Values[col.Key] = cardItem?.Value switch
                {
                    string s => s,
                    _ => null
                };
            }

            if (fieldMap.TryGetValue("__rawId", out var rawItem) && rawItem.Value is int rawId)
            {
                row.Values["__rawId"] = new ReportCell { Value = rawId };
                row.IsSelected = selectedIds.Contains(rawId);
            }

            data.Rows.Add(row);
        }

        return data;
    }

    private static int? ExtractRowId(ReportTableRow row)
    {
        if (row.Values.TryGetValue("__rawId", out var cell) && cell.Value is int id && id > 0)
            return id;
        return null;
    }

    private HashSet<int> GetSelectedIdSnapshot()
    {
        lock (_selectionLock)
        {
            return new HashSet<int>(_selectedIds);
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
