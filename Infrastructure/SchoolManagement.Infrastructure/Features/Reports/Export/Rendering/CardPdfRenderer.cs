using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class CardPdfRenderer : IPdfRenderer
    {
        private readonly IEnumerable<ICardPdfRenderer> _cardRenderers;

        public CardPdfRenderer(IEnumerable<ICardPdfRenderer> cardRenderers)
        {
            _cardRenderers = cardRenderers;
        }

        public bool CanRender(ReportResult result) => result is CardReportResult;

        public void Render(IDocumentContainer document, ReportResult data)
        {
            if (data is not CardReportResult cardData)
                throw new InvalidOperationException("CardPdfRenderer requires a CardReportResult.");

            BuildCardPages(document, cardData);
        }

        private void BuildCardPages(IDocumentContainer container, CardReportResult data)
        {
            CardSheetLayout layout = data.Layout ?? new CardSheetLayout();
            List<ReportItemGroup> cardGroups = data.CardGroups ?? [];

            int columns = Math.Max(1, layout.Columns);
            int rows = Math.Max(1, layout.Rows);
            int cardsPerPage = Math.Max(1, layout.CardsPerPage);
            int pageCount = cardGroups.Count == 0
                ? 1
                : (int)Math.Ceiling(cardGroups.Count / (double)cardsPerPage);

            PageSize pageSize = ResolvePageSize(layout.PageSize, layout.Landscape);

            float horizontalSpacing = Math.Max(0, layout.HorizontalSpacing);
            float verticalSpacing = Math.Max(0, layout.VerticalSpacing);

            float availableWidth = pageSize.Width - (layout.Margin * 2) - (horizontalSpacing * (columns - 1));
            float availableHeight = pageSize.Height - (layout.Margin * 2) - (verticalSpacing * (rows - 1));

            if (availableWidth <= 0 || availableHeight <= 0)
            {
                throw new InvalidOperationException(
                    $"Card layout '{layout.PageSize}' with margin {layout.Margin} and spacing " +
                    $"{horizontalSpacing}x{verticalSpacing} leaves no printable area.");
            }

            float slotWidth = availableWidth / columns;
            float slotHeight = availableHeight / rows;

            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                int pageStart = pageIndex * cardsPerPage;

                container.Page(page =>
                {
                    page.Size(pageSize);
                    page.Margin(layout.Margin);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    if (layout.ShowHeader)
                    {
                        page.Header().PaddingBottom(6).AlignCenter().Text(
                            layout.HeaderText ?? data.Title)
                            .FontSize(12)
                            .Bold();
                    }

                    page.Content().Column(column =>
                    {
                        column.Spacing(verticalSpacing);

                        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                        {
                            column.Item().Row(row =>
                            {
                                row.Spacing(horizontalSpacing);

                                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                                {
                                    int cardIndex = pageStart + (rowIndex * columns) + columnIndex;
                                    ReportItemGroup? cardGroup = cardIndex < cardGroups.Count
                                        ? cardGroups[cardIndex]
                                        : null;

                                    row.ConstantItem(slotWidth)
                                        .Height(slotHeight)
                                        .Element(cell =>
                                        {
                                            if (cardGroup == null)
                                            {
                                                cell.Element(_ => { });
                                                return;
                                            }

                                            CardRenderContext context = CreateRenderContext(cardGroup, slotWidth, slotHeight);
                                            ICardPdfRenderer? renderer = _cardRenderers.FirstOrDefault(c => c.CanRender(data));

                                            if (renderer == null) 
                                                throw new InvalidOperationException(
                                                    "The report data couldn't be exported due to having no renderer can handle it\n" +
                                                    $"The report tag: {data.ReportTag.ToString()}");

                                            renderer.Render(cell, cardGroup, context);
                                        });
                                }
                            });
                        }
                    });

                    if (layout.ShowFooter)
                    {
                        page.Footer().PaddingTop(6).AlignCenter().Text(
                            layout.FooterText ?? string.Empty)
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    }
                });
            }
        }

        private static PageSize ResolvePageSize(string? pageSizeKey, bool landscape)
        {
            PageSize pageSize = (pageSizeKey ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "A3" => PageSizes.A3,
                "A4" => PageSizes.A4,
                "A5" => PageSizes.A5,
                "LETTER" => PageSizes.Letter,
                "LEGAL" => PageSizes.Legal,
                _ => PageSizes.A4,
            };

            return landscape ? pageSize.Landscape() : pageSize;
        }

        private static CardRenderContext CreateRenderContext(ReportItemGroup cardGroup, float slotWidth, float slotHeight)
        {
            float designWidth = Math.Max(1, cardGroup.Width);
            float designHeight = Math.Max(1, cardGroup.Height);

            float scale = Math.Min(slotWidth / designWidth, slotHeight / designHeight);
            float renderWidth = designWidth * scale;
            float renderHeight = designHeight * scale;

            return new CardRenderContext
            {
                SlotWidth = slotWidth,
                SlotHeight = slotHeight,
                ScaleX = scale,
                ScaleY = scale,
                OffsetX = Math.Max(0, (slotWidth - renderWidth) / 2f),
                OffsetY = Math.Max(0, (slotHeight - renderHeight) / 2f),
            };
        }
    }
}
