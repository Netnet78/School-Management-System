using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class TablePdfRenderer : IPdfRenderer
    {
        public bool CanRender(ReportResult result) => result is TableReportResult;

        public void Render(IDocumentContainer document, ReportResult data)
        {
            if (data is not TableReportResult tableData)
                throw new InvalidOperationException("TablePdfRenderer requires a TableReportResult.");

            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, tableData));
                page.Content().Element(c => ComposeTableContent(c, tableData));
                page.Footer().Element(c => ComposeFooter(c));
            });
        }

        private static void ComposeHeader(IContainer container, TableReportResult data)
        {
            container.Column(column =>
            {
                column.Item().Text(data.Title)
                    .FontSize(18)
                    .Bold()
                    .AlignCenter();

                if (!string.IsNullOrEmpty(data.SubTitle))
                {
                    column.Item().Text(data.SubTitle)
                        .FontSize(12)
                        .AlignCenter();
                }

                column.Item().Text($"Generated: {data.GeneratedDate:dd/MM/yyyy HH:mm}")
                    .FontSize(9)
                    .AlignRight()
                    .FontColor(Colors.Grey.Medium);

                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingVertical(5);
            });
        }

        private static void ComposeTableContent(IContainer container, TableReportResult data)
        {
            container.Table(table =>
            {
                var columns = data.Columns;

                table.ColumnsDefinition(c =>
                {
                    foreach (var col in columns)
                    {
                        float width = col.Width > 0 ? (float)col.Width : 100f;
                        c.RelativeColumn(width / 100f);
                    }
                });

                table.Header(header =>
                {
                    foreach (var col in columns)
                    {
                        header.Cell().Element(CellStyle).Text(
                            !string.IsNullOrEmpty(col.HeaderKhmer) ? col.HeaderKhmer : col.Header)
                            .Bold().FontColor(Colors.White).FontSize(9);
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container
                            .Background(Colors.Blue.Darken2)
                            .PaddingVertical(4)
                            .PaddingHorizontal(4)
                            .AlignCenter()
                            .AlignMiddle();
                    }
                });

                foreach (var row in data.Rows)
                {
                    foreach (var col in columns)
                    {
                        var cell = row.GetValueOrDefault(col.Key);
                        var displayText = cell?.Value?.ToString() ?? "";

                        table.Cell().Element(DataCellStyle).Text(displayText).FontSize(8);
                    }
                }

                static IContainer DataCellStyle(IContainer container)
                {
                    return container
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingVertical(3)
                        .PaddingHorizontal(4);
                }
            });

            if (data.Summary?.Count > 0)
            {
                container.PaddingTop(15).Column(summary =>
                {
                    summary.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    summary.Item().PaddingTop(5);

                    foreach (var kvp in data.Summary)
                    {
                        summary.Item().Row(row =>
                        {
                            row.AutoItem().Text($"{kvp.Key}: ").Bold().FontSize(10);
                            row.AutoItem().Text(kvp.Value?.ToString() ?? "").FontSize(10);
                        });
                    }
                });
            }
        }

        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium))
                .Text(text =>
                {
                    text.Span("School Management System");
                    text.Span(" | ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
        }
    }
}
