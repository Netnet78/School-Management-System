namespace SchoolManagement.Presentation.Shared.Models
{
    public class CameraFrame : IDisposable
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
