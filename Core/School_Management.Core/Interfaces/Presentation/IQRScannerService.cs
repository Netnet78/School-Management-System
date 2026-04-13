namespace School_Management.Core.Interfaces.Presentation
{
    public interface IQRScannerService
    {
        public string? Decode(byte[] bytes);
    }
}
