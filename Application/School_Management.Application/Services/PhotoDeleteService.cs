using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class PhotoDeleteService : IPhotoDeleteService
    {
        private readonly ISettingsService settingsService;
        private readonly IS3Service s3Service;

        public PhotoDeleteService(ISettingsService settingsService, IS3Service s3Service)
        {
            this.settingsService = settingsService;
            this.s3Service = s3Service;
        }

        /// <summary>
        /// Deletes the student photo located at the specified path and returns the path of the deleted photo.
        /// </summary>
        /// <remarks>The method checks if the photo exists in the 'photo/students' directory within the
        /// application's base directory before attempting to delete it.</remarks>
        /// <param name="path">The path of the student photo to be deleted. This must be a valid file path pointing to an existing photo.</param>
        /// <returns>The path of the deleted student photo.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified path does not point to an existing photo.</exception>
        public async Task<ReturnResponse> DeleteStudentPhoto(string photoKey)
        {
            Settings config = settingsService.GetAllSettings();
            string photoDirectory = Path.Combine(config.StudentPhotoFolderPath);
            string destination = Path.Combine(photoDirectory, photoKey);
            if (Path.Exists(destination))
            {
                File.Delete(destination);
            }
            ReturnResponse response = await s3Service.DeleteFile(photoKey, config.StudentPhotoFolderBucketPath);

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
