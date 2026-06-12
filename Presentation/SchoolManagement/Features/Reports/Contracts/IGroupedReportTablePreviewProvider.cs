using System.ComponentModel;
using SchoolManagement.Presentation.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Contracts
{
    public interface IGroupedReportTablePreviewProvider : INotifyPropertyChanged
    {
        GroupedReportTableData? GroupedTableData { get; }
    }
}
