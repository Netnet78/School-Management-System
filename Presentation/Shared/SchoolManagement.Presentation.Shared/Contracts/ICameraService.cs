namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface ICameraService
    {
        event Action<BitmapInfo>? FrameReady;
        CameraItem[] GetCameras();
        void Start(CameraItem item);
        void Stop();
    }
}
