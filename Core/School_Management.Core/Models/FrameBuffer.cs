namespace School_Management.Core.Models
{
    public class FrameBuffer : IDisposable
    {
        public byte[] Data { get; set; } = [];
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }

        public void Dispose()
        {
            Data = [];
        }
    }
}
