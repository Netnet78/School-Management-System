namespace SchoolManagement.Application.Features.Files.Services
{
    public class PhotoUploadService : IPhotoUploadService
    {
        private readonly ISettingsService _settings;
        private readonly IS3Service _s3Service;
        private readonly IStudentPhotoRepository _studentPhotoRepository;
        private readonly IEmployeePhotoRepository _employeePhotoRepository;

        public PhotoUploadService(
            ISettingsService settings,
            IS3Service s3Service,
            IStudentPhotoRepository studentPhotoRepository,
            IEmployeePhotoRepository employeePhotoRepository)
        {
            _settings = settings;
            _s3Service = s3Service;
            _studentPhotoRepository = studentPhotoRepository;
            _employeePhotoRepository = employeePhotoRepository;
        }

        public async Task<ReturnResponse<FileObject>> UploadStudentPhoto(string path, Candidate student)
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

            FileStatus fileStatus = returnResponse.Status == Status.Success
                ? FileStatus.Uploaded
                : FileStatus.LocalOnly;

            StudentPhoto? existingPhoto = await _studentPhotoRepository.GetByIdAsync(student.Id);

            if (existingPhoto != null)
            {
                existingPhoto.Key = fileName;
                existingPhoto.LastAttempt = DateTime.UtcNow;
                existingPhoto.LocalPath = destination;
                existingPhoto.FileStatus = fileStatus;
                await _studentPhotoRepository.UpdateAsync(existingPhoto);
            }
            else
            {
                StudentPhoto studentPhoto = new()
                {
                    Id = student.Id,
                    Student = student,
                    FileStatus = fileStatus,
                    Key = fileName,
                    LastAttempt = DateTime.UtcNow,
                    LocalPath = destination
                };
                await _studentPhotoRepository.AddAsync(studentPhoto);
            }

            return new()
            {
                Status = returnResponse.Status == Status.Success ? Status.Success : Status.Failed,
                Message = returnResponse.Status == Status.Success
                    ? "Photo uploaded successfully"
                    : $"Photo saved locally. S3 upload failed.\n{returnResponse.Message}",
                Value = new FileObject(destination)
            };
        }

        public async Task<ReturnResponse<FileObject>> UploadEmployeePhoto(string path, Employee employee)
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

            FileStatus fileStatus = returnResponse.Status == Status.Success
                ? FileStatus.Uploaded
                : FileStatus.LocalOnly;

            EmployeePhoto? existingPhoto = await _employeePhotoRepository.GetByIdAsync(employee.Id);

            if (existingPhoto != null)
            {
                existingPhoto.Key = fileName;
                existingPhoto.LastAttempt = DateTime.UtcNow;
                existingPhoto.LocalPath = destination;
                existingPhoto.FileStatus = fileStatus;
                await _employeePhotoRepository.UpdateAsync(existingPhoto);
            }
            else
            {
                EmployeePhoto employeePhoto = new()
                {
                    Id = employee.Id,
                    Employee = employee,
                    FileStatus = fileStatus,
                    Key = fileName,
                    LastAttempt = DateTime.UtcNow,
                    LocalPath = destination
                };
                await _employeePhotoRepository.AddAsync(employeePhoto);
            }

            return new()
            {
                Status = returnResponse.Status == Status.Success ? Status.Success : Status.Failed,
                Message = returnResponse.Status == Status.Success
                    ? "Photo uploaded successfully"
                    : $"Photo saved locally. S3 upload failed.\n{returnResponse.Message}",
                Value = new FileObject(destination)
            };
        }
    }
}

