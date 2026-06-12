using ClosedXML.Excel;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Contracts
{
    public interface IExcelRenderer
    {
        bool CanRender(ReportResult result);

        XLWorkbook Render(ReportResult data);

        //public string TemplatePath { get; }
    }
}
