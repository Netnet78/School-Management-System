using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Reports.Models
{
    public class GroupedReportTableData
    {
        public ObservableCollection<GroupTab> Tabs { get; init; } = [];

        public bool HasData => Tabs.Count > 0;
    }

    public class GroupTab
    {
        public string Header { get; init; } = string.Empty;

        public List<ReportTableColumnInfo> Columns { get; init; } = [];

        public ObservableCollection<ReportTableRow> Rows { get; init; } = [];

        public Dictionary<string, object?> Summary { get; init; } = [];
    }
}
