using System.ComponentModel;
using SchoolManagement.Presentation.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGroupedReportTablePreviewProvider : INotifyPropertyChanged
    {
        GroupedReportTableData? GroupedTableData { get; }
    }
}
