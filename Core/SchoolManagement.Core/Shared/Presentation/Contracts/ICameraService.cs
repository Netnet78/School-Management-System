using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface ICameraService
    {
        event Action<CameraFrame>? FrameReady;
        CameraItem[] GetCameras();
        void Start(CameraItem item);
        void Stop();
    }
}
