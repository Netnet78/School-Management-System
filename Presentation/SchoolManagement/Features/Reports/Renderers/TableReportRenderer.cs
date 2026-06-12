using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Renderers
{
    public class TableReportRenderer : IReportRenderer
    {
        public bool CanRender(ReportResult result) => result is TableReportResult;

        public async Task<object> Render(ReportResult result)
        {
            if (result is not TableReportResult t)
                throw new InvalidOperationException("TableReportRenderer requires a TableReportResult.");

            var data = new ReportTableData();

            return await Task.Run(() =>
            {
                foreach (ReportColumn col in t.Columns)
                {
                    data.Columns.Add(new ReportTableColumnInfo
                    {
                        Key = col.Key,
                        DisplayName = col.HeaderKhmer ?? col.Header,
                        Width = col.Width,
                        FontSize = col.FontSize,
                        IsBold = col.IsBold,
                        Alignment = col.Alignment,
                        ForegroundColor = col.ForegroundColor,
                        BackgroundColor = col.BackgroundColor,
                    });
                }

                foreach (var row in t.Rows)
                {
                    var tableRow = new ReportTableRow
                    {
                        Values = row.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    };
                    data.Rows.Add(tableRow);
                }

                return data;
            });
        }
    }
}
