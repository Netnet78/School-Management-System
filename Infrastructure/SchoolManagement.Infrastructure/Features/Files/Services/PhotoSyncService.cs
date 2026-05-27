namespace SchoolManagement.Infrastructure.Features.Files.Services
{
    public class PhotoSyncService : IPhotoSyncService
    {
        private readonly IStudentPhotoRepository _studentPhotoRepo;
        private readonly IEmployeePhotoRepository _employeePhotoRepo;
        private readonly ISettingsService _settingsService;
        private readonly IS3Service _cloud;
        private bool _isInitialized = false;

        public PhotoSyncService(
            IStudentPhotoRepository studentPhotoRepo,
            IS3Service cloud,
            IEmployeePhotoRepository employeePhotoRepo,
            ISettingsService settingsService)
        {
            _studentPhotoRepo = studentPhotoRepo;
            _employeePhotoRepo = employeePhotoRepo;
            _cloud = cloud;
            _settingsService = settingsService;
        }

        public async Task ProcessPendingUploads(CancellationToken token)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                return;
            }

            Settings config = _settingsService.GetAllSettings();

            IEnumerable<StudentPhoto> pendingStudents = await _studentPhotoRepo.GetPendingUploads(token);
            IEnumerable<EmployeePhoto> pendingEmployees = await _employeePhotoRepo.GetPendingUploads(token);

            foreach (StudentPhoto photo in pendingStudents)
            {
                ReturnResponse result = await _cloud.UploadFile(photo.LocalPath!, config.StudentPhotoFolderBucketPath, token);

                if (result.Status == Status.Success)
                {
                    photo.FileStatus = FileStatus.Uploaded;
                    await _studentPhotoRepo.UpdateAsync(photo);
                }
            }

            foreach (EmployeePhoto photo in pendingEmployees)
            {
                ReturnResponse result = await _cloud.UploadFile(photo.LocalPath!, config.EmployeePhotoFolderBucketPath, token);

                if (result.Status == Status.Success)
                {
                    photo.FileStatus = FileStatus.Uploaded;
                    await _employeePhotoRepo.UpdateAsync(photo);
                }
            }

        }

        public async Task ProcessPendingDeletes(CancellationToken token)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                return;
            }

            Settings config = _settingsService.GetAllSettings();

            IEnumerable<StudentPhoto> pendingStudents = await _studentPhotoRepo.GetPendingDeletes(token);
            IEnumerable<EmployeePhoto> pendingEmployees = await _employeePhotoRepo.GetPendingDeletes(token);

            foreach (StudentPhoto photo in pendingStudents)
            {
                ReturnResponse returnResponse = await _cloud.DeleteFile(photo.Key!, config.StudentPhotoFolderBucketPath, token);

                if (returnResponse.Status != Status.Success) return;

                await _studentPhotoRepo.DeleteAsync(photo);
            }

            foreach (EmployeePhoto photo in pendingEmployees)
            {
                ReturnResponse returnResponse = await _cloud.DeleteFile(photo.Key!, config.EmployeePhotoFolderBucketPath, token);

                if (returnResponse.Status != Status.Success) return;

                await _employeePhotoRepo.DeleteAsync(photo);
            }

        }
    }
}
