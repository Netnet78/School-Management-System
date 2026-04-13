using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class PhotoUploadService : IPhotoUploadService
    {
        private readonly ISettingsService _settings;
        private readonly IS3Service _s3Service;

        public PhotoUploadService(ISettingsService settings, IS3Service s3Service)
        {
            _settings = settings;
            _s3Service = s3Service;
        }

        public async Task<FileObject> UploadStudentPhoto(string path)
        {
            Settings config = _settings.GetAllSettings();

            string photoDirectory = config.StudentPhotoFolderPath;

            if (!Directory.Exists(photoDirectory))
            {
                Directory.CreateDirectory(photoDirectory);
            }

            string uuid = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(path);
            string fileName = $"{uuid}{extension}";
            string destination = Path.Combine(photoDirectory, fileName);

            File.Copy(path, destination, true);
            ReturnResponse returnResponse = await _s3Service.UploadFile(destination, config.StudentPhotoFolderBucketPath);

            if (returnResponse.Status == ReturnStatus.Failed)
            {
                throw new Exception(returnResponse.Message);
            }

            return new FileObject(destination);
        }

        public async Task<FileObject> UploadEmployeePhoto(string path)
        {
            Settings config = _settings.GetAllSettings();

            string photoDirectory = config.EmployeePhotoFolderPath;

            if (!Directory.Exists(photoDirectory))
            {
                Directory.CreateDirectory(photoDirectory);
            }

            string uuid = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(path);
            string fileName = $"{uuid}{extension}";
            string destination = Path.Combine(photoDirectory, fileName);

            File.Copy(path, destination, true);
            ReturnResponse returnResponse = await _s3Service.UploadFile(destination, config.EmployeePhotoFolderBucketPath);
            if (returnResponse.Status == ReturnStatus.Failed)
            {
                throw new Exception(returnResponse.Message);
            }

            return new FileObject(destination);
        }
    }
}