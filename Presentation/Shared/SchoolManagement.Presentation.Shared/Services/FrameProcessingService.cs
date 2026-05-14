using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SchoolManagement.Presentation.Shared.Services
{

    public class FrameProcessingService : IFrameProcessingService
    {
        private WriteableBitmap? _wb;
        private byte[] _buffer = [];

        public object ProcessFrame(CameraFrame frame)
        {
            try
            {
                _buffer = frame.Data;
                return RenderFrame(frame);
            }
            finally
            {
                frame.Dispose();
            }
        }

        private void InitializeWb(CameraFrame frame)
        {
            _wb = new(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr24, null);
        }

        private WriteableBitmap RenderFrame(CameraFrame frame)
        {
            if (_wb == null ||
                _wb.PixelWidth != frame.Width ||
                _wb.PixelHeight != frame.Height)
            {
                InitializeWb(frame);
            }

            _wb!.Lock();

            _wb.WritePixels(
                new Int32Rect(0, 0, frame.Width, frame.Height),
                _buffer,
                frame.Stride,
                0
            );

            _wb.Unlock();

            return _wb;
        }
    }
}
