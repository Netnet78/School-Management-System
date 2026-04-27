using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Presentation
{
    public interface IQRScannerService
    {
        public string? Decode(CameraFrame frame);
    }
}
