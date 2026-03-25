using School_Management.Core.Interfaces;
using School_Management.Core.Models;
using System.Text.Json;

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
        public async Task<FileObject> DeleteStudentPhoto(string photoKey)
        {
            Settings config = settingsService.GetAllSettings();
            string photoDirectory = Path.Combine(config.StudentPhotoFolderPath);
            string destination = Path.Combine(photoDirectory, photoKey);
            if (Path.Exists(destination))
            {
                File.Delete(destination);
            }
            await s3Service.DeleteFile(photoKey, config.StudentPhotoFolderBucketPath);
            return new(destination);
        }
        public async Task<FileObject> DeleteEmployeePhoto(string photoKey)
        {
            Settings config = settingsService.GetAllSettings();
            string photoDirectory = Path.Combine(config.EmployeePhotoFolderPath);
            string destination = Path.Combine(photoDirectory, photoKey);
            if (Path.Exists(destination))
            {
                File.Delete(destination);
            }
            await s3Service.DeleteFile(photoKey, config.EmployeePhotoFolderBucketPath);
            return new(destination);
        }
    }
}
