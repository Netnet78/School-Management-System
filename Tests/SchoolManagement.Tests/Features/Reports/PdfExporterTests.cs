using System.Text;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Features.Reports.Export;
using SchoolManagement.Infrastructure.Features.Reports.Export.Rendering;

namespace SchoolManagement.Tests.Features.Reports
{
    public class PdfExporterTests
    {
        private readonly PdfExporter _exporter = new(new IPdfRenderer[] { new CardPdfRenderer([new StudentCardRenderer()]) });

        [Fact]
        public async Task ExportAsync_ForStudentCardReport_ReturnsPdfBytes()
        {
            var pdfBytes = await _exporter.ExportAsync(CreateCardReportResult());

            Assert.NotEmpty(pdfBytes);
            Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdfBytes, 0, 5));
        }

        private static CardReportResult CreateCardReportResult()
        {
            var sealPath = Path.Combine(AppContext.BaseDirectory, "Images", "dbs_red.png");
            var sealBytes = File.ReadAllBytes(sealPath);

            return new CardReportResult
            {
                Title = "Student Card",
                ReportTag = "student-card",
                GeneratedDate = new DateTime(2026, 5, 30, 8, 30, 0, DateTimeKind.Local),
                CardGroups =
                [
                    new CardDefinition
                    {
                        Width = 709,
                        Height = 945,
                        Items =
                        [
                            new CardItem
                            {
                                XPos = 150,
                                YPos = 320,
                                Value = "Test Student",
                                Width = 260,
                                Height = 32,
                                FontSize = 14,
                                IsBold = true,
                                FontColor = "#2E86DE",
                                FontFamily = "Noto Sans Khmer",
                            },
                            new CardItem
                            {
                                XPos = 48,
                                YPos = 740,
                                Value = sealBytes,
                                Width = 120,
                                Height = 120,
                            },
                        ],
                    },
                ],
                Layout = new CardSheetLayout
                {
                    PageSize = "A4",
                    Landscape = true,
                    Columns = 3,
                    Rows = 2,
                    Margin = 14f,
                    HorizontalSpacing = 0f,
                    VerticalSpacing = 0f,
                    ShowHeader = false,
                    ShowFooter = false,
                },
            };
        }
    }
}
