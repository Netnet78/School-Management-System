using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IS3Service
    {
        Task<ReturnResponse> DeleteFile(string fileKey, string? folder = null, CancellationToken cancellationToken = default);
        Task<ReturnResponse> UploadFile(string filePath, string? folder = null, CancellationToken cancellationToken = default);
        Task<ReturnResponse> DownloadFile(string key, string savePath, string? folder = null, CancellationToken cancellationToken = default);
    }
}
