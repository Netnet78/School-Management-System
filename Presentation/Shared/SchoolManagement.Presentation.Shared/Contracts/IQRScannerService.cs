namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface IQRScannerService
    {
        public string? Decode(BitmapInfo frame);
    }
}
