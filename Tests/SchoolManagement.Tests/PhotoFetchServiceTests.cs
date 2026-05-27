using Moq;

namespace SchoolManagement.Tests
{
    public class PhotoFetchServiceTests
    {
        private readonly Mock<ISettingsService> _settingsMock;
        private readonly Mock<IS3Service> _s3Mock;
        private readonly PhotoFetchService _service;
        private readonly Settings _settings;

        public PhotoFetchServiceTests()
        {
            _settingsMock = new Mock<ISettingsService>();
            _s3Mock = new Mock<IS3Service>();

            _settings = new Settings
            {
                StudentPhotoFolderPath = "/tmp/student-photos",
                StudentPhotoFolderBucketPath = "photos/students",
                EmployeePhotoFolderPath = "/tmp/employee-photos",
                EmployeePhotoFolderBucketPath = "photos/employees"
            };

            _settingsMock.Setup(s => s.GetAllSettings()).Returns(_settings);

            _service = new PhotoFetchService(_settingsMock.Object, _s3Mock.Object);
        }

        // ====== Student ======

        [Fact]
        public async Task GetStudentPhoto_NullKey_ReturnsRejected()
        {
            var result = await _service.GetStudentPhoto(null!);

            Assert.Equal(Status.Rejected, result.Status);
            _s3Mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStudentPhoto_EmptyKey_ReturnsRejected()
        {
            var result = await _service.GetStudentPhoto(string.Empty);

            Assert.Equal(Status.Rejected, result.Status);
            _s3Mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStudentPhoto_FileExistsLocally_ReturnsSuccessWithoutDownload()
        {
            string photoKey = "existing-photo.jpg";
            string localPath = Path.Combine(_settings.StudentPhotoFolderPath, photoKey);
            Directory.CreateDirectory(_settings.StudentPhotoFolderPath);
            await File.WriteAllTextAsync(localPath, "fake-image");

            var result = await _service.GetStudentPhoto(photoKey, FileLocationOptions.LocalAndOnline);

            Assert.Equal(Status.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(localPath, result.Value!.FilePath);
            _s3Mock.Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task GetStudentPhoto_FileNotLocal_S3DownloadSucceeds_ReturnsSuccess()
        {
            string photoKey = "remote-photo.jpg";
            _s3Mock.Setup(s => s.DownloadFile(photoKey, _settings.StudentPhotoFolderPath, _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            var result = await _service.GetStudentPhoto(photoKey);

            Assert.Equal(Status.Success, result.Status);
            _s3Mock.Verify(s => s.DownloadFile(photoKey, _settings.StudentPhotoFolderPath, _settings.StudentPhotoFolderBucketPath, default), Times.Once);
        }

        [Fact]
        public async Task GetStudentPhoto_FileNotLocal_S3DownloadFails_ReturnsFailed()
        {
            string photoKey = "missing-photo.jpg";
            _s3Mock.Setup(s => s.DownloadFile(photoKey, _settings.StudentPhotoFolderPath, _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Failed, Message = "Not found" });

            var result = await _service.GetStudentPhoto(photoKey);

            Assert.Equal(Status.Failed, result.Status);
        }

        [Fact]
        public async Task GetStudentPhoto_ConfigPathEmpty_ReturnsRejected()
        {
            _settings.StudentPhotoFolderPath = string.Empty;
            var result = await _service.GetStudentPhoto("any-key");

            Assert.Equal(Status.Rejected, result.Status);
            _s3Mock.VerifyNoOtherCalls();
        }

        // ====== Employee ======

        [Fact]
        public async Task GetEmployeePhoto_NullKey_ReturnsRejected()
        {
            var result = await _service.GetEmployeePhoto(null!);

            Assert.Equal(Status.Rejected, result.Status);
        }

        [Fact]
        public async Task GetEmployeePhoto_EmptyKey_ReturnsRejected()
        {
            var result = await _service.GetEmployeePhoto(string.Empty);

            Assert.Equal(Status.Rejected, result.Status);
        }

        [Fact]
        public async Task GetEmployeePhoto_FileExistsLocally_ReturnsSuccessWithoutDownload()
        {
            string photoKey = "emp-existing.jpg";
            string localPath = Path.Combine(_settings.EmployeePhotoFolderPath, photoKey);
            Directory.CreateDirectory(_settings.EmployeePhotoFolderPath);
            await File.WriteAllTextAsync(localPath, "fake");

            var result = await _service.GetEmployeePhoto(photoKey);

            Assert.Equal(Status.Success, result.Status);
            Assert.Equal(localPath, result.Value!.FilePath);
            _s3Mock.Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task GetEmployeePhoto_FileNotLocal_S3DownloadSucceeds_ReturnsSuccess()
        {
            string photoKey = "emp-remote.jpg";
            _s3Mock.Setup(s => s.DownloadFile(photoKey, _settings.EmployeePhotoFolderPath, _settings.EmployeePhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            var result = await _service.GetEmployeePhoto(photoKey);

            Assert.Equal(Status.Success, result.Status);
            _s3Mock.Verify(s => s.DownloadFile(photoKey, _settings.EmployeePhotoFolderPath, _settings.EmployeePhotoFolderBucketPath, default), Times.Once);
        }

        [Fact]
        public async Task GetEmployeePhoto_FileNotLocal_S3DownloadFails_ReturnsFailed()
        {
            string photoKey = "emp-missing.jpg";
            _s3Mock.Setup(s => s.DownloadFile(photoKey, _settings.EmployeePhotoFolderPath, _settings.EmployeePhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Failed });

            var result = await _service.GetEmployeePhoto(photoKey);

            Assert.Equal(Status.Failed, result.Status);
        }

        [Fact]
        public async Task GetEmployeePhoto_ConfigPathEmpty_ReturnsRejected()
        {
            _settings.EmployeePhotoFolderPath = string.Empty;
            var result = await _service.GetEmployeePhoto("any-key");

            Assert.Equal(Status.Rejected, result.Status);
        }
    }
}
