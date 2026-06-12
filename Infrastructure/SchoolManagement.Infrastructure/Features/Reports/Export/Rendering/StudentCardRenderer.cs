using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SchoolManagement.Assets;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Features.Reports.Models;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class StudentCardRenderer : ICardPdfRenderer
    {
        private const double RenderScale = 300.0 / 72.0; // around 4.167f
        private static readonly Dictionary<string, SKTypeface> FontCache = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> FontFileMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Khmer OS Muol Light"] = "khmer-os-muol-light.ttf",
            ["Khmer OS Battambang"] = "khmer-os-battambang.ttf",
            ["Khmer OS Bokor"] = "khmer-os-bokor.ttf",
            ["Khmer OS Muol"] = "khmer-os-muol.ttf",
            ["Times New Roman"] = "times-new-roman.ttf",
        };


        public bool CanRender(ReportResult result) => result is CardReportResult r && r.ReportTag == ReportTag.StudentCard;
        public void Render(IContainer container, ReportItemGroup cardGroup, CardRenderContext? context = null)
        {
            LoadFonts();

            float offsetX = Math.Max(0f, context?.OffsetX ?? 0f);
            float offsetY = Math.Max(0f, context?.OffsetY ?? 0f);

            if (!string.IsNullOrWhiteSpace(cardGroup.TemplateFileFilePath))
            {
                string templatePath = cardGroup.TemplateFileFilePath;

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException(
                        $"Card template file was not found: {templatePath}",
                        templatePath);
                }

                byte[] bytes = RenderCard(cardGroup, context, offsetX, offsetY);
                container.PaddingLeft(offsetX).PaddingTop(offsetY).Image(bytes).FitArea();
            }
            else
            {
                byte[] bytes = RenderCard(cardGroup, context, offsetX, offsetY);
                container.PaddingLeft(offsetX).PaddingTop(offsetY).Image(bytes).FitArea();
            }
        }

        private static byte[] RenderCard(ReportItemGroup cardGroup, CardRenderContext? context, float offsetX, float offsetY)
        {
            float scale = Math.Min(context?.ScaleX ?? 1f, context?.ScaleY ?? 1f) * (float)RenderScale;

            int cardWidth = (int)(cardGroup.Width * scale);
            int cardHeight = (int)(cardGroup.Height * scale);
            using SKBitmap sKBitmap = new(cardWidth, cardHeight);
            using SKCanvas canvas = new(sKBitmap);
            canvas.Clear(SKColors.White);

            if (!string.IsNullOrWhiteSpace(cardGroup.TemplateFileFilePath))
            {
                using SKBitmap template = SKBitmap.Decode(cardGroup.TemplateFileFilePath);
                canvas.DrawBitmap(template, new SKRect(0, 0, cardWidth, cardHeight));
            }

            foreach (CardItem item in cardGroup.Items)
            {
                float x = item.XPos * scale;
                float y = item.YPos * scale;

                if (item.Value is byte[] imageBytes)
                {
                    using SKBitmap photo = SKBitmap.Decode(imageBytes);
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
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
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

            if (FontCache.TryGetValue(fontFamily, out var cached)) return cached;

            if (FontFileMap.TryGetValue(fontFamily, out var fileName))
            {
                var path = Path.Combine(ResourcePaths.Fonts, fileName);
                if (File.Exists(path))
                {
                    SKTypeface typeface = SKTypeface.FromFile(path);
                    FontCache[fontFamily] = typeface;
                    return typeface;
                }
            }

            return null;
        }

        public static void LoadFonts()
        {
            foreach (var (family, fileName) in FontFileMap)
            {
                var path = Path.Combine(ResourcePaths.Fonts, fileName);
                if (File.Exists(path) && !FontCache.ContainsKey(family))
                {
                    FontCache[family] = SKTypeface.FromFile(path);
                }
            }
        }
    }
}
