using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Views;

namespace SchoolManagement.Presentation.Features.Reports.Providers;

public partial class StudentCardTableReportProvider : ObservableObject, IReportViewProvider, IReportTablePreviewProvider
{
    private readonly IReportGenerator _generator;
    private readonly IReportFilterViewModel? _filterVm;
    private CardReportResult? _lastCardResult;
    private object? _lastFilter;
    private int _generationId;

    private readonly HashSet<int> _selectedIds = [];
    private bool _isBatchUpdating;

    private ReportTableData? _fullTableData;

    private const int DefaultPageSize = 10;

    public ReportTag ReportTypeKey => ReportTag.StudentCard;

    public IReportFilterViewModel? FilterViewModel => _filterVm;

    public UserControl PreviewView { get; }

    [ObservableProperty]
    private ReportTableData? _tableData;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _summaryText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaxPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int _totalCount;

    [ObservableProperty]
    private bool _isAllSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaxPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaxPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int _pageSize = DefaultPageSize;

    public int MaxPage => PageSize > 0 ? Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize)) : 1;

    public string PageInfo => $"{CurrentPage} / {MaxPage} ({TotalCount})";

    public StudentCardTableReportProvider(
        IReportGenerator generator,
        IReportFilterViewModel? filterVm)
    {
        _generator = generator;
        _filterVm = filterVm;
        PreviewView = new ReportTablePreviewView { DataContext = this };
    }

    public void SelectStudent(int studentId)
    {
        if (!_isBatchUpdating)
        {
            _selectedIds.Add(studentId);
            UpdateIsAllSelected();
        }
    }

    public void DeselectStudent(int studentId)
    {
        if (!_isBatchUpdating)
        {
            _selectedIds.Remove(studentId);
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
                int? id = ExtractStudentId(row);
                if (id.HasValue)
                    _selectedIds.Add(id.Value);
                row.IsSelected = true;
            }
        }
        else
        {
            foreach (var row in TableData.Rows)
            {
                int? id = ExtractStudentId(row);
                if (id.HasValue)
                    _selectedIds.Remove(id.Value);
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

    private static int? ExtractStudentId(ReportTableRow row)
    {
        if (row.Values.TryGetValue("__studentRawId", out var cell) && cell.Value is int id && id > 0)
            return id;
        return null;
    }

    [RelayCommand]
    private async Task GoToNextPage()
    {
        if (CurrentPage < MaxPage)
        {
            CurrentPage++;
            SliceTableDataToCurrentPage();
        }
    }

    [RelayCommand]
    private async Task GoToPreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            SliceTableDataToCurrentPage();
        }
    }

    private void SliceTableDataToCurrentPage()
    {
        if (_fullTableData == null) return;

        int offset = (CurrentPage - 1) * PageSize;
        int count = Math.Min(PageSize, _fullTableData.Rows.Count - offset);

        var pageData = new ReportTableData
        {
            HasSelection = true,
            Columns = _fullTableData.Columns,
            Rows = new ObservableCollection<ReportTableRow>(
                _fullTableData.Rows.Skip(offset).Take(count))
        };

        foreach (var row in pageData.Rows)
        {
            int? id = ExtractStudentId(row);
            if (id.HasValue)
            {
                row.IsSelected = _selectedIds.Contains(id.Value);
            }
        }

        int selected = pageData.Rows.Count(r => r.IsSelected);
        IsAllSelected = pageData.Rows.Count > 0 && selected == pageData.Rows.Count;

        TableData = pageData;
    }

    public async Task GenerateAsync(object filter, CancellationToken cancellationToken = default)
    {
        int generationId = Interlocked.Increment(ref _generationId);

        var result = (CardReportResult)await _generator.GenerateAsync(filter, cancellationToken);
        _lastCardResult = result;
        _lastFilter = filter;

        if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
            return;

        string summaryText = BuildSummaryText(result);
        var allTableData = ConvertCardToTable(result);

        if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
            return;

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (cancellationToken.IsCancellationRequested || generationId != Volatile.Read(ref _generationId))
                return;

            allTableData.HasSelection = true;
            _fullTableData = allTableData;
            int fullCount = result.Summary?.TryGetValue("__totalCount", out var tc) == true && tc is int totalCount
                ? totalCount
                : allTableData.Rows.Count;
            TotalCount = fullCount;

            int totalPages = Math.Max(1, (int)Math.Ceiling((double)fullCount / PageSize));
            if (CurrentPage > totalPages)
            {
                CurrentPage = totalPages;
            }

            SliceTableDataToCurrentPage();
            HasData = allTableData.HasData;
            SummaryText = summaryText;
        });
    }

    public bool CanExport(IReportExporter exporter) => exporter.FormatName == "PDF";

    public async Task ExportAsync(IReportExporter exporter, string filePath, CancellationToken cancellationToken = default)
    {
        if (!CanExport(exporter))
            throw new InvalidOperationException($"Student card report does not support {exporter.FormatName} export.");

        if (_lastCardResult == null) return;

        CardReportResult exportResult = _lastCardResult;

        if (_selectedIds.Count > 0 && exportResult.CardGroups != null)
        {
            var filteredGroups = exportResult.CardGroups
                .Where(g => _selectedIds.Contains(ExtractStudentIdFromGroup(g)))
                .ToList();

            exportResult = new CardReportResult
            {
                Title = exportResult.Title,
                SubTitle = exportResult.SubTitle,
                GeneratedDate = exportResult.GeneratedDate,
                ReportTag = exportResult.ReportTag,
                CardGroups = filteredGroups,
                Layout = exportResult.Layout,
                Summary = new Dictionary<string, object>
                {
                    ["__totalCount"] = filteredGroups.Count,
                    ["ចំនួនសិស្សសរុប"] = filteredGroups.Count,
                },
            };
        }

        await exporter.ExportToFileAsync(exportResult, filePath, cancellationToken).ConfigureAwait(false);
    }

    private static int ExtractStudentIdFromGroup(ReportItemGroup group)
    {
        foreach (var item in group.Items)
        {
            if (item.FieldName == "StudentId" && item.Value is string studentIdStr)
            {
                if (studentIdStr.StartsWith("SDBS") && int.TryParse(studentIdStr.AsSpan(4), out var id))
                    return id;
            }
        }
        return 0;
    }

    private static ReportTableData ConvertCardToTable(CardReportResult cardResult)
    {
        var data = new ReportTableData();

        data.Columns.Add(new ReportTableColumnInfo { Key = "photo", DisplayName = "រូបថត", Width = 80 });
        data.Columns.Add(new ReportTableColumnInfo { Key = "studentId", DisplayName = "លេខសម្គាល់", Width = 130 });
        data.Columns.Add(new ReportTableColumnInfo { Key = "fullName", DisplayName = "ឈ្មោះពេញ", Width = 260 });
        data.Columns.Add(new ReportTableColumnInfo { Key = "gender", DisplayName = "ភេទ", Width = 80 });
        data.Columns.Add(new ReportTableColumnInfo { Key = "dateOfBirth", DisplayName = "ថ្ងៃខែឆ្នាំកំណើត", Width = 160 });
        data.Columns.Add(new ReportTableColumnInfo { Key = "class", DisplayName = "ថ្នាក់", Width = 160 });
        data.Columns.Add(new ReportTableColumnInfo { Key = "department", DisplayName = "ផ្នែក", Width = 190 });
        data.Columns.Add(new ReportTableColumnInfo { Key = "academicYear", DisplayName = "ឆ្នាំសិក្សា", Width = 150 });

        if (cardResult.CardGroups == null)
            return data;

        foreach (ReportItemGroup group in cardResult.CardGroups)
        {
            var row = new ReportTableRow();

            byte[]? photoBytes = null;
            string? studentId = null, fullName = null, gender = null, dob = null,
                    className = null, department = null, academicYear = null;
            int rawStudentId = 0;

            foreach (var item in group.Items)
            {
                switch (item.FieldName)
                {
                    case "Photo" when item.Value is byte[] bytes:
                        photoBytes = bytes;
                        break;
                    case "AcademicYear" when item.Value is string text:
                        academicYear = text;
                        break;
                    case "FullName" when item.Value is string text:
                        fullName = text;
                        break;
                    case "Gender" when item.Value is string text:
                        gender = text;
                        break;
                    case "StudentId" when item.Value is string text:
                        studentId = text;
                        if (text.StartsWith("SDBS") && int.TryParse(text.AsSpan(4), out var id))
                            rawStudentId = id;
                        break;
                    case "DateOfBirth" when item.Value is string text:
                        dob = text;
                        break;
                    case "Class" when item.Value is string text:
                        className = text;
                        break;
                    case "Department" when item.Value is string text:
                        department = text;
                        break;
                }
            }

            row.Values["__studentRawId"] = new ReportCell { Value = rawStudentId };
            row.Values["photo"] = new ReportCell { Value = photoBytes };
            row.Values["studentId"] = studentId;
            row.Values["fullName"] = fullName;
            row.Values["gender"] = gender;
            row.Values["dateOfBirth"] = dob;
            row.Values["class"] = className;
            row.Values["department"] = department;
            row.Values["academicYear"] = academicYear;

            data.Rows.Add(row);
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
