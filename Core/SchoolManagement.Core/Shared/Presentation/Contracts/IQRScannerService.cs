using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface IQRScannerService
    {
        public string? Decode(CameraFrame frame);
    }
}
