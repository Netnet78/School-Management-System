using Moq;

namespace SchoolManagement.Tests.Features.Photos
{
    public class PhotoUploadServiceTests
    {
        private readonly Mock<ISettingsService> _settingsMock;
        private readonly Mock<IS3Service> _s3Mock;
        private readonly Mock<IStudentPhotoRepository> _studentPhotoRepoMock;
        private readonly Mock<IEmployeePhotoRepository> _employeePhotoRepoMock;
        private readonly PhotoUploadService _service;
        private readonly Settings _settings;

        public PhotoUploadServiceTests()
        {
            _settingsMock = new Mock<ISettingsService>();
            _s3Mock = new Mock<IS3Service>();
            _studentPhotoRepoMock = new Mock<IStudentPhotoRepository>();
            _employeePhotoRepoMock = new Mock<IEmployeePhotoRepository>();

            _settings = new Settings
            {
                StudentPhotoFolderPath = "/tmp/student-photos",
                StudentPhotoFolderBucketPath = "photos/students",
                EmployeePhotoFolderPath = "/tmp/employee-photos",
                EmployeePhotoFolderBucketPath = "photos/employees"
            };

            _settingsMock.Setup(s => s.GetAllSettings()).Returns(_settings);

            _service = new PhotoUploadService(
                _settingsMock.Object,
                _s3Mock.Object,
                _studentPhotoRepoMock.Object,
                _employeePhotoRepoMock.Object);
        }

        [Fact]
        public async Task UploadStudentPhoto_NewPhoto_S3Success_AddsWithUploadedStatus()
        {
            var candidate = new Candidate { Id = 1 };
            string sourcePath = "/tmp/source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake-image-data");

            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync((StudentPhoto?)null);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            var result = await _service.UploadStudentPhoto(sourcePath, candidate);

            Assert.Equal(Status.Success, result.Status);
            Assert.NotNull(result.Value);
            _studentPhotoRepoMock.Verify(r => r.AddAsync(It.Is<StudentPhoto>(p =>
                p.Id == candidate.Id &&
                p.FileStatus == FileStatus.Uploaded)), Times.Once);
            _studentPhotoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<StudentPhoto>()), Times.Never);
        }

        [Fact]
        public async Task UploadStudentPhoto_ExistingPhoto_S3Success_UpdatesWithUploadedStatus()
        {
            var candidate = new Candidate { Id = 2 };
            string sourcePath = "/tmp/source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake-image-data");

            var existingPhoto = new StudentPhoto { Id = 2, Key = "old-key", LocalPath = "/tmp/old.jpg" };
            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync(existingPhoto);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            var result = await _service.UploadStudentPhoto(sourcePath, candidate);

            Assert.Equal(Status.Success, result.Status);
            Assert.NotNull(result.Value);
            _studentPhotoRepoMock.Verify(r => r.UpdateAsync(It.Is<StudentPhoto>(p =>
                p.Id == existingPhoto.Id &&
                p.FileStatus == FileStatus.Uploaded)), Times.Once);
            _studentPhotoRepoMock.Verify(r => r.AddAsync(It.IsAny<StudentPhoto>()), Times.Never);
        }

        [Fact]
        public async Task UploadStudentPhoto_S3Fails_SetsLocalOnly()
        {
            var candidate = new Candidate { Id = 3 };
            string sourcePath = "/tmp/source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake-image-data");

            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync((StudentPhoto?)null);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Failed });

            var result = await _service.UploadStudentPhoto(sourcePath, candidate);

            Assert.Equal(Status.Failed, result.Status);
            _studentPhotoRepoMock.Verify(r => r.AddAsync(It.Is<StudentPhoto>(p =>
                p.FileStatus == FileStatus.LocalOnly)), Times.Once);
        }

        [Fact]
        public async Task UploadStudentPhoto_ExistingPhoto_S3Fails_UpdatesWithLocalOnly()
        {
            var candidate = new Candidate { Id = 4 };
            string sourcePath = "/tmp/source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake-image-data");

            var existing = new StudentPhoto { Id = 4, Key = "old", LocalPath = "/tmp/old.jpg" };
            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync(existing);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Failed });

            var result = await _service.UploadStudentPhoto(sourcePath, candidate);

            Assert.Equal(Status.Failed, result.Status);
            _studentPhotoRepoMock.Verify(r => r.UpdateAsync(It.Is<StudentPhoto>(p =>
                p.FileStatus == FileStatus.LocalOnly)), Times.Once);
        }

        [Fact]
        public async Task UploadStudentPhoto_EmptyPhotoKey_UsesNewGuid()
        {
            var candidate = new Candidate { Id = 5 };
            string sourcePath = "/tmp/source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake-image-data");

            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync((StudentPhoto?)null);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            StudentPhoto? captured = null;
            _studentPhotoRepoMock.Setup(r => r.AddAsync(It.IsAny<StudentPhoto>()))
                .Callback<StudentPhoto>(p => captured = p);

            await _service.UploadStudentPhoto(sourcePath, candidate);

            Assert.NotNull(captured);
            Assert.Matches(@"^[0-9a-f\-]+\.jpg$", captured!.Key!);
        }

        // ====== Employee ======

        [Fact]
        public async Task UploadEmployeePhoto_NewPhoto_S3Success_AddsWithUploadedStatus()
        {
            var employee = new Employee { Id = 10 };
            string sourcePath = "/tmp/emp-source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake");

            _employeePhotoRepoMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync((EmployeePhoto?)null);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.EmployeePhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            var result = await _service.UploadEmployeePhoto(sourcePath, employee);

            Assert.Equal(Status.Success, result.Status);
            _employeePhotoRepoMock.Verify(r => r.AddAsync(It.Is<EmployeePhoto>(p =>
                p.Id == employee.Id && p.FileStatus == FileStatus.Uploaded)), Times.Once);
        }

        [Fact]
        public async Task UploadEmployeePhoto_ExistingPhoto_S3Success_UpdatesWithUploadedStatus()
        {
            var employee = new Employee { Id = 11 };
            string sourcePath = "/tmp/emp-source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake");

            var existing = new EmployeePhoto { Id = 11, Key = "old-key" };
            _employeePhotoRepoMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(existing);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.EmployeePhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            var result = await _service.UploadEmployeePhoto(sourcePath, employee);

            Assert.Equal(Status.Success, result.Status);
            _employeePhotoRepoMock.Verify(r => r.UpdateAsync(It.Is<EmployeePhoto>(p =>
                p.FileStatus == FileStatus.Uploaded)), Times.Once);
            _employeePhotoRepoMock.Verify(r => r.AddAsync(It.IsAny<EmployeePhoto>()), Times.Never);
        }

        [Fact]
        public async Task UploadEmployeePhoto_S3Fails_SetsLocalOnly()
        {
            var employee = new Employee { Id = 12 };
            string sourcePath = "/tmp/emp-source.jpg";
            await File.WriteAllTextAsync(sourcePath, "fake");

            _employeePhotoRepoMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync((EmployeePhoto?)null);
            _s3Mock.Setup(s => s.UploadFile(It.IsAny<string>(), _settings.EmployeePhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Failed });

            var result = await _service.UploadEmployeePhoto(sourcePath, employee);

            Assert.Equal(Status.Failed, result.Status);
            _employeePhotoRepoMock.Verify(r => r.AddAsync(It.Is<EmployeePhoto>(p =>
                p.FileStatus == FileStatus.LocalOnly)), Times.Once);
        }
    }
}
