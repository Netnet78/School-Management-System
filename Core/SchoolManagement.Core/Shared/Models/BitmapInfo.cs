namespace SchoolManagement.Core.Shared.Models
{
    /// <summary>
    /// Represents bitmap image data and associated metadata such as width, height, and stride.
    /// </summary>
    /// <remarks>The BitmapInfo class provides a simple container for raw bitmap pixel data and its
    /// dimensions. It implements IDisposable to allow clearing sensitive image data from memory when no longer needed.
    /// The Data property should contain pixel data in a format determined by the consuming application.</remarks>
    public class BitmapInfo : IDisposable
    {
        public byte[] Data { get; set; } = [];
        public int Width { get; set; }
        public int Height { get; set; }

        public int Stride { get; set; }

        public BitmapInfo() { }

        public BitmapInfo(byte[] data)
        {
            Data = data;
        }

        public void Dispose()
        {
            Array.Clear(Data, 0, Data.Length);
            Data = [];
            GC.SuppressFinalize(this);
        }
    }
}
