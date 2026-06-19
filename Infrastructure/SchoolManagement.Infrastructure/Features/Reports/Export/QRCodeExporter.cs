using SchoolManagement.Assets;
using SkiaSharp;
using ZXing;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace SchoolManagement.Infrastructure.Features.Reports.Export;

public static class QRCodeExporter
{
    public static byte[] GenerateQrPng(string value, string label, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];

        BarcodeWriterPixelData writer = new()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new()
            {
                Height = height,
                Width = width,
                PureBarcode = true,
            },
        };

        writer.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);

        unsafe
        {
            PixelData pixelData = writer.Write(value);

            SKImageInfo info = new(
                pixelData.Width,
                pixelData.Height,
                SKColorType.Bgra8888,
                SKAlphaType.Premul);

            using SKBitmap bitmap = new(info);

            fixed (byte* ptr = pixelData.Pixels)
            {
                bitmap.SetPixels((nint)ptr);
            }

            using SKCanvas canvas = new(bitmap);

            string logoPath = Path.Combine(ResourcePaths.Images, "dbslogo_filled.png");

            // Draw logo if provided
            if (!string.IsNullOrWhiteSpace(logoPath)
                && File.Exists(logoPath))
            {
                using SKBitmap logoBitmap = SKBitmap.Decode(logoPath);

                int logoSize = width / 5;

                int x = (width - logoSize) / 2;
                int y = (height - logoSize) / 2;

                SKRect logoRect = new(
                    x,
                    y,
                    x + logoSize,
                    y + logoSize);

                // White circular background
                using SKPaint paint = new()
                {
                    Color = SKColors.Black,
                    IsAntialias = true
                };

                float radius = logoSize / 2f + 10;

                using SKTypeface typeface = SKTypeface.FromFile(Path.Combine(ResourcePaths.Fonts, "noto-sans-khmer.ttf"));
                using SKFont font = new(typeface);

                canvas.DrawText(
                    label,
                    width / 2,
                    height + 20,
                    SKTextAlign.Center,
                    font,
                    paint);

                paint.Color = SKColors.White;

                canvas.DrawCircle(
                    width / 2f,
                    height / 2f,
                    radius,
                    paint);

                // Draw logo
                canvas.DrawBitmap(logoBitmap, logoRect);
            }

            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }
    }
    public static byte[] GenerateQrPng(string value, int width, int height)
    {
        return GenerateQrPng(value, string.Empty, width, height);
    }
}
