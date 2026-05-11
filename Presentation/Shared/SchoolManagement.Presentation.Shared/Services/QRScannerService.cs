using SchoolManagement.Core.Shared.Models;
using System.Diagnostics;
using ZXing;
using ZXing.Windows.Compatibility;
using SchoolManagement.Core.Shared.Presentation.Contracts;

namespace SchoolManagement.Presentation.Shared.Services
{
    public class QRScannerService : IQRScannerService
    {
        private readonly BarcodeReader _barcodeReader;

        public QRScannerService()
        {
            _barcodeReader = new()
            {
                AutoRotate = true,
                Options = new()
                {
                    TryHarder = true,
                    PureBarcode = false,
                }
            };
        }

        public string? Decode(CameraFrame frame)
        {
            Result result = _barcodeReader.Decode(
                frame.Data,
                frame.Width,
                frame.Height,
                RGBLuminanceSource.BitmapFormat.BGR24
            );

            if (result != null && !string.IsNullOrWhiteSpace(result.Text))
            {
                Debug.WriteLine($"Scanned Code: {result.Text}");
                return result.Text;
            }
            else
            {
                Debug.WriteLine("No barcode value detected");
                return null;
            }
        }
    }
}
