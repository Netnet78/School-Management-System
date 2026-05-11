using OpenCvSharp;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Application.Services
{
    public class PhotoDeleteService : IPhotoDeleteService
    {
        private readonly ISettingsService settingsService;
        private readonly IS3Service s3Service;
        private readonly IStudentPhotoRepository studentPhotoRepository;

        public PhotoDeleteService(ISettingsService settingsService,
                                  IS3Service s3Service,
                                  IStudentPhotoRepository studentPhotoRepository)
        {
            this.settingsService = settingsService;
            this.s3Service = s3Service;
            this.studentPhotoRepository = studentPhotoRepository;
        }

        /// <summary>
        /// Deletes the student photo located at the specified path and returns the path of the deleted photo.
        /// </summary>
        /// <remarks>The method checks if the photo exists in the 'photo/students' directory within the
        /// application's base directory before attempting to delete it.</remarks>
        /// <param name="student"></param>
        /// <returns>The path of the deleted student photo.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified path does not point to an existing photo.</exception>
        public async Task<ReturnResponse> DeleteStudentPhoto(Candidate student)
        {
            string photoKey = student.PhotoKey;

            if (string.IsNullOrWhiteSpace(photoKey))
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "សិស្សគ្មានទិន្នន័យរួបភាពដែលត្រូវលុបទេ"
                };
            }

            Settings config = settingsService.GetAllSettings();
            string photoDirectory = Path.Combine(config.StudentPhotoFolderPath);
            string destination = Path.Combine(photoDirectory, photoKey);
            if (Path.Exists(destination))
            {
                File.Delete(destination);
            }

            StudentPhoto studentPhoto = new()
            {
                Student = student,
                FileStatus = FileStatus.PendingDelete,
                Key = student.PhotoKey,
                LastAttempt = DateTime.UtcNow,
                LocalPath = destination
            };

            ReturnResponse response = await s3Service.DeleteFile(photoKey, config.StudentPhotoFolderBucketPath);

            if (response.Status == Status.Success && student.Photo != null)
            {
                await studentPhotoRepository.DeleteAsync(student.Photo);
            }
            else
            {
                studentPhoto.FileStatus = FileStatus.PendingDelete;
                await studentPhotoRepository.UpdateAsync(studentPhoto);
            }

            return new()
            {
                Status = response.Status,
                Message = response.Message,
            };
        }
        public async Task<ReturnResponse> DeleteEmployeePhoto(string photoKey)
        {
            Settings config = settingsService.GetAllSettings();
            string photoDirectory = Path.Combine(config.EmployeePhotoFolderPath);
            string destination = Path.Combine(photoDirectory, photoKey);
            if (Path.Exists(destination))
            {
                File.Delete(destination);
            }

            ReturnResponse response = await s3Service.DeleteFile(photoKey, config.EmployeePhotoFolderBucketPath);

            return new()
            {
                Message = response.Message,
                Status = response.Status,
            };
        }
    }
}
