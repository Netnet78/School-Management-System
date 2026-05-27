namespace SchoolManagement.Application.Features.Files.Services
{
    public class PhotoDeleteService : IPhotoDeleteService
    {
        private readonly ISettingsService settingsService;
        private readonly IS3Service s3Service;
        private readonly IStudentPhotoRepository studentPhotoRepository;
        private readonly IEmployeePhotoRepository employeePhotoRepository;

        public PhotoDeleteService(ISettingsService settingsService,
                                  IS3Service s3Service,
                                  IStudentPhotoRepository studentPhotoRepository,
                                  IEmployeePhotoRepository employeePhotoRepository)
        {
            this.settingsService = settingsService;
            this.s3Service = s3Service;
            this.studentPhotoRepository = studentPhotoRepository;
            this.employeePhotoRepository = employeePhotoRepository;
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
            StudentPhoto? existingPhoto = await studentPhotoRepository.GetByIdAsync(student.Id);

            if (existingPhoto == null || string.IsNullOrWhiteSpace(existingPhoto.Key))
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "រូបភាពនៃសិស្សមិនអាចគ្មានទិន្នន័យ​បានទេ!"
                };
            }

            Settings config = settingsService.GetAllSettings();
            string photoDirectory = Path.Combine(config.StudentPhotoFolderPath);
            string destination = Path.Combine(photoDirectory, existingPhoto.Key);
            if (Path.Exists(destination))
            {
                File.Delete(destination);
            }

            existingPhoto.FileStatus = FileStatus.PendingDelete;
            existingPhoto.LastAttempt = DateTime.UtcNow;

            ReturnResponse response = await s3Service.DeleteFile(existingPhoto.Key, config.StudentPhotoFolderBucketPath);

            if (response.Status == Status.Success)
            {
                await studentPhotoRepository.DeleteAsync(existingPhoto);
            }
            else
            {
                await studentPhotoRepository.UpdateAsync(existingPhoto);
            }

            return new()
            {
                Status = response.Status,
                Message = response.Message,
            };
        }
        public async Task<ReturnResponse> DeleteEmployeePhoto(Employee employee)
        {
            EmployeePhoto? existingPhoto = await employeePhotoRepository.GetByIdAsync(employee.Id);

            if (existingPhoto == null || string.IsNullOrWhiteSpace(existingPhoto.Key))
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "រូបភាពនៃបុគ្គលិកមិនអាចគ្មានទិន្នន័យ​បានទេ!"
                };
            }

            Settings config = settingsService.GetAllSettings();
            string photoDirectory = Path.Combine(config.EmployeePhotoFolderPath);
            string destination = Path.Combine(photoDirectory, existingPhoto.Key);
            if (Path.Exists(destination))
            {
                File.Delete(destination);
            }

            existingPhoto.FileStatus = FileStatus.PendingDelete;
            existingPhoto.LastAttempt = DateTime.UtcNow;

            ReturnResponse response = await s3Service.DeleteFile(existingPhoto.Key, config.EmployeePhotoFolderBucketPath);

            if (response.Status == Status.Success)
            {
                await employeePhotoRepository.DeleteAsync(existingPhoto);
            }
            else
            {
                await employeePhotoRepository.UpdateAsync(existingPhoto);
            }

            return new()
            {
                Status = response.Status,
                Message = response.Message,
            };
        }
    }
}


