using ClosedXML.Excel;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Features.Reports.Export.Rendering;

namespace SchoolManagement.Infrastructure.Features.Reports.Export
{
    public class ExcelExporter : IReportExporter
    {
        private readonly ExcelTemplateRenderer _templateRenderer;

        public ExcelExporter(ExcelTemplateRenderer templateRenderer)
        {
            _templateRenderer = templateRenderer;
        }

        public string FormatName => "Excel";

        public string FileExtension => ".xlsx";

        public string? TemplatePath { get; set; }

        public async Task<byte[]> ExportAsync(ReportResult data, CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream();
            await ExportToStreamAsync(data, stream, cancellationToken);
            return stream.ToArray();
        }

        public async Task ExportToFileAsync(ReportResult data, string filePath, CancellationToken cancellationToken = default)
        {
            using var workbook = CreateWorkbook(data);
            workbook.SaveAs(filePath);
            await Task.CompletedTask;
        }

        private async Task ExportToStreamAsync(ReportResult data, MemoryStream stream, CancellationToken cancellationToken)
        {
            using var workbook = CreateWorkbook(data);
            workbook.SaveAs(stream);
            await Task.CompletedTask;
        }

        private XLWorkbook CreateWorkbook(ReportResult data)
        {
            var resolvedTemplate = data.TemplatePath ?? TemplatePath;

            if (data.Layout == ReportLayout.Card)
            {
                return CreateCardWorkbook(data, resolvedTemplate);
            }

            return CreateTableWorkbook(data, resolvedTemplate);
        }

        private XLWorkbook CreateCardWorkbook(ReportResult data, string? resolvedTemplate)
        {
            if (string.IsNullOrEmpty(resolvedTemplate) || !File.Exists(resolvedTemplate))
            {
                throw new InvalidOperationException(
                    "Excel export for card layout requires a template file. " +
                    "Set TemplatePath on the ReportResult or ExcelExporter, and ensure the file exists.");
            }

            var workbook = new XLWorkbook(resolvedTemplate);
            _templateRenderer.FillCells(workbook, data);
            return workbook;
        }

        private XLWorkbook CreateTableWorkbook(ReportResult data, string? resolvedTemplate)
        {
            XLWorkbook workbook;

            if (!string.IsNullOrEmpty(resolvedTemplate) && File.Exists(resolvedTemplate))
            {
                workbook = new XLWorkbook(resolvedTemplate);
            }
            else
            {
                workbook = new XLWorkbook();
            }

            var ws = workbook.Worksheets.FirstOrAdd("Report");
            ws.Clear();

            int currentRow = 1;

            // Title
            ws.Cell(currentRow, 1).Value = data.Title;
            ws.Range(currentRow, 1, currentRow, data.Columns.Count).Merge();
            ws.Cell(currentRow, 1).Style
                .Font.SetBold(true)
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            currentRow += 2;

            // Subtitle & date
            if (!string.IsNullOrEmpty(data.SubTitle))
            {
                ws.Cell(currentRow, 1).Value = data.SubTitle;
                ws.Cell(currentRow, data.Columns.Count).Value = data.GeneratedDate.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(currentRow, data.Columns.Count).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                currentRow++;
            }

            // Column headers
            for (int c = 0; c < data.Columns.Count; c++)
            {
                var col = data.Columns[c];
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
            foreach (var row in data.Rows)
            {
                for (int c = 0; c < data.Columns.Count; c++)
                {
                    var col = data.Columns[c];
                    var value = row.GetValueOrDefault(col.Key);
                    var cell = ws.Cell(currentRow, c + 1);
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    if (value == null)
                    {
                        cell.Value = "";
                    }
                    else if (value is decimal || value is double || value is float || value is int)
                    {
                        cell.Value = Convert.ToDouble(value);
                    }
                    else
                    {
                        cell.Value = value.ToString();
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

    internal static class WorksheetExtensions
    {
        public static IXLWorksheet FirstOrAdd(this IXLWorksheets worksheets, string name)
        {
            if (worksheets.TryGetWorksheet(name, out var ws))
                return ws;

            return worksheets.Add(name);
        }
    }
}
