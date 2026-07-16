using SkiaSharp;

namespace SchoolManagement.Infrastructure.Shared.Extensions
{
    public static class ImageExtensions
    {
        public static (int Width, int Height) GetImageSize(this byte[] imageBytes)
        {
            using SKCodec codec = SKCodec.Create(
                new MemoryStream(imageBytes));

            return (
                codec.Info.Width,
                codec.Info.Height);
        }
    }
}
