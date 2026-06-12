using ClosedXML.Excel;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Features.Reports.Export;
using SchoolManagement.Infrastructure.Features.Reports.Export.Rendering;

namespace SchoolManagement.Tests.Features.Reports
{
    public class ExcelExporterTests
    {
        private readonly static IEnumerable<IExcelRenderer> _renderers = [new DefaultExcelTemplateRenderer()];
        private readonly ExcelExporter _exporter = new(_renderers);

        [Fact]
        public async Task ExportToFileAsync_WithoutTemplate_UsesDefaultLayout()
        {
            var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");

            try
            {
                await _exporter.ExportToFileAsync(CreateTableResult(), outputPath);

                using var workbook = new XLWorkbook(outputPath);
                var ws = workbook.Worksheets.First();

                Assert.Equal("Student Report", ws.Cell(1, 1).GetString());
                Assert.Equal("Name", ws.Cell(4, 1).GetString());
                Assert.Equal("Score", ws.Cell(4, 2).GetString());
                Assert.Equal("Alice", ws.Cell(5, 1).GetString());
                Assert.Equal("95", ws.Cell(5, 2).GetString());
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [Fact]
        public async Task ExportToFileAsync_ForCardReport_Throws()
        {
            var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");

            try
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _exporter.ExportToFileAsync(new CardReportResult() { ReportTag = ReportTag.StudentCard }, outputPath));

                Assert.Contains("This report type cannot be exported to Excel", exception.Message);
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        private static TableReportResult CreateTableResult()
        {
            return new TableReportResult
            {
                ReportTag = ReportTag.StudentRoster,
                Title = "Student Report",
                SubTitle = "Export Test",
                GeneratedDate = new DateTime(2026, 5, 29, 12, 0, 0, DateTimeKind.Utc),
                Columns =
                [
                    new() { Key = "name", Header = "Name", Width = 150 },
                    new() { Key = "score", Header = "Score", DataType = typeof(int), Width = 80 },
                ],
                Rows =
                [
                    new Dictionary<string, ReportCell>
                    {
                        ["name"] = "Alice",
                        ["score"] = 95,
                    }
                ],
                Summary = new Dictionary<string, object>
                {
                    ["total"] = 1,
                },
            };
        }
    }
}
