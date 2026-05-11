using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Application.Services
{
    public class PhotoUploadService : IPhotoUploadService
    {
        private readonly ISettingsService _settings;
        private readonly IS3Service _s3Service;
        private readonly IStudentPhotoRepository _studentPhotoRepository;

        public PhotoUploadService(
            ISettingsService settings,
            IS3Service s3Service,
            IStudentPhotoRepository studentPhotoRepository)
        {
            _settings = settings;
            _s3Service = s3Service;
            _studentPhotoRepository = studentPhotoRepository;
        }

        public async Task<FileObject> UploadStudentPhoto(string path, Candidate student)
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

            StudentPhoto studentPhoto = new()
            {
                Student = student,
                FileStatus = FileStatus.PendingUpload,
                Key = fileName,
                LastAttempt = DateTime.UtcNow,
                LocalPath = destination
            };

            ReturnResponse returnResponse = await _s3Service.UploadFile(destination, config.StudentPhotoFolderBucketPath);

            if (returnResponse.Status == Status.Success)
            {
                studentPhoto.FileStatus = FileStatus.Uploaded;
            }
            else
            {
                studentPhoto.FileStatus = FileStatus.LocalOnly;
            }

            if (student.Photo != null)
            {
                await _studentPhotoRepository.UpdateAsync(studentPhoto);
            }
            else
            {
                await _studentPhotoRepository.AddAsync(studentPhoto);
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
            if (returnResponse.Status == Status.Failed)
            {
                throw new Exception(returnResponse.Message);
            }

            return new FileObject(destination);
        }
    }
}