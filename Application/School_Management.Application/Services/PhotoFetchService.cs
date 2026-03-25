using School_Management.Core.Interfaces;
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

        public async Task<FileObject?> GetStudentPhoto(string photoKey)
        {
            Settings config = _settings.GetAllSettings();
            string path = Path.Combine(config.StudentPhotoFolderPath, photoKey);

            try
            {
                if (!File.Exists(path))
                {
                    await _s3Service.DownloadFile(
                        photoKey,
                        config.StudentPhotoFolderPath,
                        config.StudentPhotoFolderBucketPath
                    );
                }
            }
            catch
            {
                return null;
            }

            return new(path);
        }

        public async Task<FileObject?> GetEmployeePhoto(string photoKey)
        {
            Settings config = _settings.GetAllSettings();
            string path = Path.Combine(config.EmployeePhotoFolderPath, photoKey);

            try
            {
                if (!File.Exists(path))
                {
                    await _s3Service.DownloadFile(
                        photoKey,
                        config.EmployeePhotoFolderPath,
                        config.EmployeePhotoFolderBucketPath
                    );
                }
            }
            catch
            {
                return null;
            }

            return new(path);
        }
    }
}