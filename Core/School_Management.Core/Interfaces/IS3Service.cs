using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Interfaces
{
    public interface IS3Service
    {
        Task DeleteFile(string fileKey, string? folder = null);
        Task UploadFile(string filePath, string? folder = null);
        Task DownloadFile(string key, string savePath, string? folder = null);
    }
}
