using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SchoolManagement.Assets;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Core.Shared.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Features.Reports.Models;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System.Collections.Concurrent;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class StudentCardRenderer : ICardPdfRenderer
    {
        private const double RenderScale = 300.0 / 72.0; // around 4.167f
        private static readonly ConcurrentDictionary<string, Lazy<SKTypeface?>> FontCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> FontFileMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Khmer OS Muol Light"] = "khmer-os-muol-light.ttf",
            ["Khmer OS Battambang"] = "khmer-os-battambang.ttf",
            ["Khmer OS Bokor"] = "khmer-os-bokor.ttf",
            ["Khmer OS Muol"] = "khmer-os-muol.ttf",
            ["Times New Roman"] = "times-new-roman.ttf",
        };

        private static readonly ConcurrentDictionary<string, Lazy<SKBitmap?>> TemplateCache = new(StringComparer.OrdinalIgnoreCase);

        public bool CanRender(ReportResult result) => result is CardReportResult r && r.ReportTag == "student-card";
        public void Render(IContainer container, CardDefinition cardGroup, CardRenderContext context)
        {
            float offsetX = Math.Max(0f, context.OffsetX);
            float offsetY = Math.Max(0f, context.OffsetY);

            byte[] bytes = RenderToBytes(cardGroup, context);
            container.PaddingLeft(offsetX).PaddingTop(offsetY).Image(bytes).FitArea();
        }

        private static byte[] RenderCard(CardDefinition cardGroup, CardRenderContext? context, float offsetX, float offsetY)
        {
            float scale = Math.Min(context?.ScaleX ?? 1f, context?.ScaleY ?? 1f) * (float)RenderScale;

            int cardWidth = (int)(cardGroup.Width * scale);
            int cardHeight = (int)(cardGroup.Height * scale);
            using SKBitmap sKBitmap = new(cardWidth, cardHeight);
            using SKCanvas canvas = new(sKBitmap);
            canvas.Clear(SKColors.White);

            string? templatePath = cardGroup.TemplateFilePath;
            if (!string.IsNullOrWhiteSpace(templatePath))
            {
                SKBitmap? template = TemplateCache.GetOrAdd(templatePath, path => new Lazy<SKBitmap?>(() =>
                    File.Exists(path) ? SKBitmap.Decode(path) : null,
                    LazyThreadSafetyMode.ExecutionAndPublication)).Value;

                if (template != null)
                    canvas.DrawBitmap(template, new SKRect(0, 0, cardWidth, cardHeight));
            }

            foreach (CardItem item in cardGroup.Items)
            {
                float x = item.XPos * scale;
                float y = item.YPos * scale;

                if (item.Value is BitmapInfo bitmapInfo)
                {
                    using SKBitmap photo = SKBitmap.Decode(bitmapInfo.Data);
                    float w = (float)(item.Width ?? photo.Width) * (float)scale;
                    float h = (float)(item.Height ?? photo.Height) * (float)scale;
                    canvas.DrawBitmap(photo, new SKRect(x, y, x + w, y + h));
                }
                else if (item.Value is string text)
                {
                    using SKFont sKFont = new()
                    {
                        Embolden = item.IsBold,
                        Size = item.FontSize * scale,
                        Subpixel = true,
                        Typeface = GetTypeface(item.FontFamily),
                    };

                    using SKPaint sKPaint = new()
                    {
                        Color = ParseColor(item.FontColor),
                        IsAntialias = true,
                        IsStroke = false,
                    };

                    SKTextAlign sKTextAlign = item.TextAlignment switch
                    {
                        TextAlignment.Left => SKTextAlign.Left,
                        TextAlignment.Right => SKTextAlign.Right,
                        TextAlignment.Center => SKTextAlign.Center,
                        _ => SKTextAlign.Left
                    };

                    float textX = item.TextAlignment switch
                    {
                        TextAlignment.Center => x + (float)(item.Width ?? 0) * (float)scale / 2f,
                        TextAlignment.Right => x + (float)(item.Width ?? 0) * (float)scale,
                        _ => x,
                    };

                    float baseline = y + sKFont.Size;

                    canvas.DrawShapedText(text, textX, baseline, sKTextAlign, sKFont, sKPaint);
                }
            }

            using var image = SKImage.FromBitmap(sKBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
            return data.ToArray();
        }

        public static SKColor ParseColor(string? hex)
        {
            if (!SKColor.TryParse(hex, out SKColor textColor))
            {
                textColor = SKColors.Black;
            }

            return textColor;
        }

        public static SKTypeface? GetTypeface(string? fontFamily)
        {
            if (string.IsNullOrWhiteSpace(fontFamily)) return null;

            return FontCache.GetOrAdd(fontFamily, family => new Lazy<SKTypeface?>(() =>
            {
                if (FontFileMap.TryGetValue(family, out var fileName))
                {
                    var path = Path.Combine(ResourcePaths.Fonts, fileName);
                    if (File.Exists(path))
                        return SKTypeface.FromFile(path);
                }
                return null;
            }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        public byte[] RenderToBytes(CardDefinition cardGroup, CardRenderContext context)
        {
            float offsetX = Math.Max(0f, context?.OffsetX ?? 0f);
            float offsetY = Math.Max(0f, context?.OffsetY ?? 0f);
            return RenderCard(cardGroup, context, offsetX, offsetY);
        }
    }
}
