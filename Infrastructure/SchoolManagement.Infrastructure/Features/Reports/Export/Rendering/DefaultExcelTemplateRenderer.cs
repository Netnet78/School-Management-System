using ClosedXML.Excel;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class DefaultExcelTemplateRenderer : IExcelRenderer
    {
        public bool CanRender(ReportResult result) => result is TableReportResult;

        public XLWorkbook Render(ReportResult data)
        {
            if (data is not TableReportResult tableData)
                throw new InvalidOperationException("DefaultExcelTemplateRenderer requires a TableReportResult.");

            XLWorkbook workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Any()
                ? workbook.Worksheets.First()
                : workbook.Worksheets.Add("Report");
            ws.Clear();

            int currentRow = 1;

            // Title
            ws.Cell(currentRow, 1).Value = tableData.Title;
            ws.Range(currentRow, 1, currentRow, tableData.Columns.Count).Merge();
            ws.Cell(currentRow, 1).Style
                .Font.SetBold(true)
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            currentRow += 2;

            // Subtitle & date
            if (!string.IsNullOrEmpty(tableData.SubTitle))
            {
                ws.Cell(currentRow, 1).Value = tableData.SubTitle;
                ws.Cell(currentRow, tableData.Columns.Count).Value = tableData.GeneratedDate.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(currentRow, tableData.Columns.Count).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                currentRow++;
            }

            // Column headers
            for (int c = 0; c < tableData.Columns.Count; c++)
            {
                var col = tableData.Columns[c];
                var header = !string.IsNullOrEmpty(col.HeaderKhmer) ? col.HeaderKhmer : col.Header;
                var cell = ws.Cell(currentRow, c + 1);
                cell.Value = header;
                cell.Style
                    .Font.SetBold(true)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(63, 81, 181))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
            currentRow++;

            // Data rows
            foreach (var row in tableData.Rows)
            {
                for (int c = 0; c < tableData.Columns.Count; c++)
                {
                    var col = tableData.Columns[c];
                    var cellValue = row.GetValueOrDefault(col.Key);
                    var cell = ws.Cell(currentRow, c + 1);
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    var rawValue = cellValue?.Value;
                    if (rawValue == null)
                    {
                        cell.Value = "";
                    }
                    else if (rawValue is decimal || rawValue is double || rawValue is float || rawValue is int)
                    {
                        cell.Value = Convert.ToDouble(rawValue);
                    }
                    else
                    {
                        cell.Value = rawValue.ToString();
                    }

                    if (col.DataType == typeof(decimal) || col.DataType == typeof(double))
                    {
                        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    }
                }
                currentRow++;
            }

            // Summary row
            if (data.Summary?.Count > 0)
            {
                currentRow++;
                foreach (var kvp in data.Summary)
                {
                    ws.Cell(currentRow, 1).Value = kvp.Key;
                    ws.Cell(currentRow, 1).Style.Font.SetBold(true);
                    ws.Cell(currentRow, 2).Value = kvp.Value?.ToString() ?? "";
                    currentRow++;
                }
            }

            // Auto-fit columns
            ws.Columns().AdjustToContents();

            return workbook;
        }

    }
}
