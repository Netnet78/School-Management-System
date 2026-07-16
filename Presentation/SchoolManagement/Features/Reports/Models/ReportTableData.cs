using System.ComponentModel;
using System.Runtime.CompilerServices;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Models
{
    public class ReportTableData
    {
        public List<ReportTableColumnInfo> Columns { get; init; } = [];

        public ObservableCollection<ReportTableRow> Rows { get; init; } = [];

        public bool HasData => Rows.Count > 0;

        public bool HasSelection { get; set; }
    }

    public class ReportTableColumnInfo
    {
        public string Key { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public double Width { get; set; } = 100;

        public double? FontSize { get; set; }

        public bool IsBold { get; set; }

        public CellAlignment Alignment { get; set; } = CellAlignment.Left;

        public string? ForegroundColor { get; set; }

        public string? BackgroundColor { get; set; }
    }

    public class ReportTableRow : INotifyPropertyChanged
    {
        public Dictionary<string, ReportCell> Values { get; init; } = [];

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
