using AForge.Video;
using AForge.Video.DirectShow;
using SchoolManagement.Presentation.Shared.Converters;
using System.Drawing;

namespace SchoolManagement.Presentation.Shared.Services;
public class CameraService : ICameraService
{
    private VideoCaptureDevice? _device;

    public event Action<BitmapInfo>? FrameReady;

    public CameraItem[] GetCameras()
    {
        FilterInfoCollection groups = new(FilterCategory.VideoInputDevice);

        return groups.Cast<FilterInfo>().Select(g => new CameraItem
        {
            MonikerString = g.MonikerString,
            Name = g.Name,
        }).ToArray();
    }

    public void Start(CameraItem item)
    {
        Stop();
        _device = new(item.MonikerString);

        _device.NewFrame += NewFrameEventHandler;
        _device.Start();

        if (_device.IsRunning == false)
        {
            throw new ArgumentException("Camera not found!");
        }
    }

    private void NewFrameEventHandler(object sender, NewFrameEventArgs eventArgs)
    {
        using Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
        BitmapInfo bufferFrame = frame.ConvertToCameraFrame();

        FrameReady?.Invoke(bufferFrame);
    }

    public void Stop()
    {
        if (_device != null)
        {
            _device.NewFrame -= NewFrameEventHandler;
            _device.SignalToStop();
            _device.WaitForStop();
            _device = null;
        }
    }
}