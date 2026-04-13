using AForge.Video.DirectShow;
using OpenCvSharp;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;

namespace School_Management.Presentation.Shared.Services
{
    public class CameraService : ICameraService
    {
        private VideoCapture? _capture;
        private CancellationTokenSource? _cancellationToken;
        private readonly int _fps = 60;

        public int SelectedDeviceIndex { get; private set; } = 0;

        public event Action<CameraFrame>? OnFrameUpdated;

        public ReturnResponse SetDevice(int deviceIndex = 0)
        {
            SelectedDeviceIndex = deviceIndex;
            _capture = new VideoCapture(SelectedDeviceIndex);

            if (!_capture.IsOpened())
                return new()
                {
                    Status = ReturnStatus.Failed,
                    Message = "មិនអាចរកឃើញកាមេរ៉ាដែលបានកំណត់នោះទេ",
                };
            return new() { Status = ReturnStatus.Success };
        }

        public void Start()
        {
            _capture?.Set(VideoCaptureProperties.Fps, _fps);
            _cancellationToken = new();
            Task.Run(() => CaptureLoop(_cancellationToken.Token));
        }

        private void CaptureLoop(CancellationToken token)
        {
            using var frame = new Mat();

            while (!token.IsCancellationRequested && _capture != null)
            {
                _capture.Read(frame);

                if (frame.Empty()) continue;

                if (!frame.IsContinuous()) continue;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateFrame(frame);
                }, System.Windows.Threading.DispatcherPriority.Render, token);
            }
        }

        private void UpdateFrame(Mat frame)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2BGRA);

            int stride = (int)frame.Step();

            CameraFrame cameraFrame = new()
            {
                Data = frame.Data,
                Stride = stride,
                Width = frame.Width,
                Height = frame.Height,
            };

            OnFrameUpdated?.Invoke(cameraFrame);
        }

        public void Stop()
        {
            _cancellationToken?.Cancel();
            _capture?.Release();
        }

        public string[] GetAvailableCameras()
        {
            FilterInfoCollection devices = new(FilterCategory.VideoInputDevice);
            string[] deviceNames = devices.Cast<FilterInfo>().Select(d => d.Name).ToArray();

            return deviceNames;
        }
    }
}
