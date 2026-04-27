using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Presentation
{
    public interface ICameraService
    {
        event Action<CameraFrame>? FrameReady;
        CameraItem[] GetCameras();
        void Start(CameraItem item);
        void Stop();
    }
}
