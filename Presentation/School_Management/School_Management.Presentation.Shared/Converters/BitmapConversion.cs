using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace School_Management.Presentation.Shared.Converters
{
    public static class BitmapConversion
    {
        public static byte[] ConvertToByteArray(this Bitmap bitmap)
        {
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                int size = Math.Abs(data.Stride) * bitmap.Height;
                byte[] buffer = new byte[size];

                Marshal.Copy(data.Scan0, buffer, 0, size);
                return buffer;
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        public static Bitmap ConvertToBitmap(this byte[] bytes)
        {
            using MemoryStream memory = new(bytes);
            return new Bitmap(memory);
        }

        public static BitmapSource ConvertToBitmapsource(this Bitmap bitmap)
        {
            using (MemoryStream memory = new())
            {
                bitmap.Save(memory, ImageFormat.MemoryBmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static BitmapSource? ConvertToBitmapsource(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            using (MemoryStream memory = new(bytes))
            {
                memory.Position = 0;

                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}
