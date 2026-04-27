using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class PhotoFetchService : IPhotoFetchService
    {
        private readonly ISettingsService _settings;
        private readonly IS3Service _s3Service;

        public PhotoFetchService(ISettingsService settings, IS3Service s3Service)
        {
            _settings = settings;
            _s3Service = s3Service;
        }

        public async Task<FileObject?> GetStudentPhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(photoKey))
            {
                return null;
            }

            Settings config = _settings.GetAllSettings();

            if (string.IsNullOrWhiteSpace(config.StudentPhotoFolderPath))
            {
                return null;
            }

            Directory.CreateDirectory(config.StudentPhotoFolderPath);
            string path = Path.Combine(config.StudentPhotoFolderPath, photoKey);

            if (!File.Exists(path) && location != FileLocationOptions.LocalOnly)
            {
                ReturnResponse returnResponse = await _s3Service.DownloadFile(
                    photoKey,
                    config.StudentPhotoFolderPath,
                    config.StudentPhotoFolderBucketPath,
                    cancellationToken
                );
                if (returnResponse.Status == ReturnStatus.Failed)
                {
                    return null;
                }
            }

            if (!File.Exists(path))
            {
                return null;
            }

            return new(path);
        }

        public async Task<FileObject?> GetEmployeePhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(photoKey))
            {
                return null;
            }

            Settings config = _settings.GetAllSettings();

            if (string.IsNullOrWhiteSpace(config.EmployeePhotoFolderPath))
            {
                return null;
            }

            Directory.CreateDirectory(config.EmployeePhotoFolderPath);
            string path = Path.Combine(config.EmployeePhotoFolderPath, photoKey);

            if (!File.Exists(path) && location != FileLocationOptions.LocalOnly)
            {
                ReturnResponse returnResponse = await _s3Service.DownloadFile(
                    photoKey,
                    config.EmployeePhotoFolderPath,
                    config.EmployeePhotoFolderBucketPath,
                    cancellationToken 
                );
                if (returnResponse.Status == ReturnStatus.Failed)
                {
                    return null;
                }
            }

            if (!File.Exists(path))
            {
                return null;
            }

            return new(path);
        }
    }
}