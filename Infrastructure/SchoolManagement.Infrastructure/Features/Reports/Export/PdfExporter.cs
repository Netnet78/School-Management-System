using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Infrastructure.Features.Reports.Export
{
    public class PdfExporter : IReportExporter
    {
        private readonly ICardRenderer _cardRenderer;

        public string FormatName => "PDF";

        public string FileExtension => ".pdf";

        public PdfExporter(ICardRenderer cardRenderer)
        {
            _cardRenderer = cardRenderer;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> ExportAsync(ReportResult data, CancellationToken cancellationToken = default)
        {
            var document = CreateDocument(data);
            var pdfBytes = document.GeneratePdf();
            return await Task.FromResult(pdfBytes);
        }

        public async Task ExportToFileAsync(ReportResult data, string filePath, CancellationToken cancellationToken = default)
        {
            var document = CreateDocument(data);
            document.GeneratePdf(filePath);
            await Task.CompletedTask;
        }

        private Document CreateDocument(ReportResult data)
        {
            return Document.Create(container =>
            {
                if (data.Layout == ReportLayout.Card)
                {
                    BuildCardPages(container, data);
                }
                else
                {
                    BuildTablePage(container, data);
                }
            });
        }

        private void BuildCardPages(IDocumentContainer container, ReportResult data)
        {
            int totalCardSides = data.CardGroups?.Count ?? 0;
            int totalCards = totalCardSides / 2;
            int cardsPerPage = 8;
            int pageCount = totalCards > 0
                ? (int)Math.Ceiling((double)totalCards / cardsPerPage)
                : 1;

            for (int p = 0; p < pageCount; p++)
            {
                int pageStart = p * cardsPerPage;
                int pageEnd = Math.Min(pageStart + cardsPerPage, totalCards);

                RenderCardSidePage(container, data, pageStart, pageEnd, sideOffset: 0);
                RenderCardSidePage(container, data, pageStart, pageEnd, sideOffset: 1);
            }
        }

        private void RenderCardSidePage(
            IDocumentContainer container, ReportResult data,
            int pageStart, int pageEnd, int sideOffset)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeCardHeader(c, data));

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    for (int i = pageStart; i < pageEnd; i++)
                    {
                        int groupIdx = i * 2 + sideOffset;
                        if (groupIdx < data.CardGroups!.Count)
                        {
                            table.Cell().Element(c => _cardRenderer.RenderCard(c, data.CardGroups![groupIdx]));
                        }
                    }
                });

                page.Footer().Element(c => ComposeCardFooter(c, pageStart / 8 + 1));
            });
        }

        private static void BuildTablePage(IDocumentContainer container, ReportResult data)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeTableContent(c, data));
                page.Footer().Element(c => ComposeFooter(c));
            });
        }

        private static void ComposeCardHeader(IContainer container, ReportResult data)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium)
                        .AlignLeft();

                    row.AutoItem().Text(data.Title)
                        .FontSize(14)
                        .Bold()
                        .AlignCenter();

                    row.RelativeItem().Text(data.GeneratedDate.ToString("dd/MM/yyyy HH:mm"))
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium)
                        .AlignRight();
                });

                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingVertical(5);
            });
        }

        private static void ComposeCardFooter(IContainer container, int cardSetNumber)
        {
            container.AlignCenter().DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium))
                .Text(text =>
                {
                    text.Span($"Card Set {cardSetNumber}");
                });
        }

        private static void ComposeHeader(IContainer container, ReportResult data)
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

        private static void ComposeTableContent(IContainer container, ReportResult data)
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
                        var value = row.GetValueOrDefault(col.Key);
                        var displayText = value?.ToString() ?? "";

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
