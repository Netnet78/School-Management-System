using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IS3Service
    {
        Task<ReturnResponse> DeleteFile(string fileKey, string? folder = null);
        Task<ReturnResponse> UploadFile(string filePath, string? folder = null);
        Task<ReturnResponse> DownloadFile(string key, string savePath, string? folder = null);
    }
}
