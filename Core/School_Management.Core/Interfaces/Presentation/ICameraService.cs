using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Presentation
{
    public interface ICameraService
    {
        public int SelectedDeviceIndex { get; }

        event Action<CameraFrame>? OnFrameUpdated;

        string[] GetAvailableCameras();
        ReturnResponse SetDevice(int index = 0);
        void Start();
        void Stop();
    }
}
