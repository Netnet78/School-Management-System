using System.Diagnostics;
using ZXing;
using ZXing.Windows.Compatibility;

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

        public string? Decode(BitmapInfo frame)
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
