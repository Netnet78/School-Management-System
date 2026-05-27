using Moq;

namespace SchoolManagement.Tests
{
    public class PhotoDeleteServiceTests
    {
        private readonly Mock<ISettingsService> _settingsMock;
        private readonly Mock<IS3Service> _s3Mock;
        private readonly Mock<IStudentPhotoRepository> _studentPhotoRepoMock;
        private readonly Mock<IEmployeePhotoRepository> _employeePhotoRepoMock;
        private readonly PhotoDeleteService _service;
        private readonly Settings _settings;

        public PhotoDeleteServiceTests()
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

            _service = new PhotoDeleteService(
                _settingsMock.Object,
                _s3Mock.Object,
                _studentPhotoRepoMock.Object,
                _employeePhotoRepoMock.Object);
        }

        // ====== Student ======

        [Fact]
        public async Task DeleteStudentPhoto_ExistingPhoto_S3Success_DeletesFromRepo()
        {
            var candidate = new Candidate { Id = 1 };
            var existing = new StudentPhoto { Id = 1, Key = "photo-key", LocalPath = "/tmp/photo.jpg" };
            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync(existing);
            _s3Mock.Setup(s => s.DeleteFile(existing.Key, _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            await File.WriteAllTextAsync(existing.LocalPath, "data");

            var result = await _service.DeleteStudentPhoto(candidate);

            Assert.Equal(Status.Success, result.Status);
            _studentPhotoRepoMock.Verify(r => r.DeleteAsync(existing), Times.Once);
            _studentPhotoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<StudentPhoto>()), Times.Never);
            Assert.False(File.Exists(existing.LocalPath));
        }

        [Fact]
        public async Task DeleteStudentPhoto_ExistingPhoto_S3Fails_MarksPendingDeleteAndUpdates()
        {
            var candidate = new Candidate { Id = 2 };
            var existing = new StudentPhoto { Id = 2, Key = "photo-key", LocalPath = "/tmp/photo2.jpg" };
            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync(existing);
            _s3Mock.Setup(s => s.DeleteFile(existing.Key, _settings.StudentPhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Failed });

            var result = await _service.DeleteStudentPhoto(candidate);

            Assert.Equal(Status.Failed, result.Status);
            _studentPhotoRepoMock.Verify(r => r.UpdateAsync(It.Is<StudentPhoto>(p =>
                p.FileStatus == FileStatus.PendingDelete)), Times.Once);
            _studentPhotoRepoMock.Verify(r => r.DeleteAsync(It.IsAny<StudentPhoto>()), Times.Never);
        }

        [Fact]
        public async Task DeleteStudentPhoto_NoExistingPhoto_ReturnsRejected()
        {
            var candidate = new Candidate { Id = 3 };
            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync((StudentPhoto?)null);

            var result = await _service.DeleteStudentPhoto(candidate);

            Assert.Equal(Status.Rejected, result.Status);
            _studentPhotoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<StudentPhoto>()), Times.Never);
            _studentPhotoRepoMock.Verify(r => r.DeleteAsync(It.IsAny<StudentPhoto>()), Times.Never);
        }

        [Fact]
        public async Task DeleteStudentPhoto_ExistingPhotoEmptyKey_ReturnsRejected()
        {
            var candidate = new Candidate { Id = 4 };
            var existing = new StudentPhoto { Id = 4, Key = string.Empty };
            _studentPhotoRepoMock.Setup(r => r.GetByIdAsync(candidate.Id)).ReturnsAsync(existing);

            var result = await _service.DeleteStudentPhoto(candidate);

            Assert.Equal(Status.Rejected, result.Status);
            _studentPhotoRepoMock.Verify(r => r.DeleteAsync(It.IsAny<StudentPhoto>()), Times.Never);
        }

        // ====== Employee ======

        [Fact]
        public async Task DeleteEmployeePhoto_ExistingPhoto_S3Success_DeletesFromRepo()
        {
            var employee = new Employee { Id = 10 };
            var existing = new EmployeePhoto { Id = 10, Key = "emp-photo-key", LocalPath = "/tmp/emp-photo.jpg" };
            _employeePhotoRepoMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(existing);
            _s3Mock.Setup(s => s.DeleteFile(existing.Key, _settings.EmployeePhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Success });

            await File.WriteAllTextAsync(existing.LocalPath, "data");

            var result = await _service.DeleteEmployeePhoto(employee);

            Assert.Equal(Status.Success, result.Status);
            _employeePhotoRepoMock.Verify(r => r.DeleteAsync(existing), Times.Once);
            _employeePhotoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<EmployeePhoto>()), Times.Never);
            Assert.False(File.Exists(existing.LocalPath));
        }

        [Fact]
        public async Task DeleteEmployeePhoto_ExistingPhoto_S3Fails_MarksPendingDelete()
        {
            var employee = new Employee { Id = 11 };
            var existing = new EmployeePhoto { Id = 11, Key = "emp-key", LocalPath = "/tmp/emp2.jpg" };
            _employeePhotoRepoMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(existing);
            _s3Mock.Setup(s => s.DeleteFile(existing.Key, _settings.EmployeePhotoFolderBucketPath, default))
                   .ReturnsAsync(new ReturnResponse { Status = Status.Failed });

            var result = await _service.DeleteEmployeePhoto(employee);

            Assert.Equal(Status.Failed, result.Status);
            _employeePhotoRepoMock.Verify(r => r.UpdateAsync(It.Is<EmployeePhoto>(p =>
                p.FileStatus == FileStatus.PendingDelete)), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployeePhoto_NoExistingPhoto_ReturnsRejected()
        {
            var employee = new Employee { Id = 12 };
            _employeePhotoRepoMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync((EmployeePhoto?)null);

            var result = await _service.DeleteEmployeePhoto(employee);

            Assert.Equal(Status.Rejected, result.Status);
            _employeePhotoRepoMock.Verify(r => r.DeleteAsync(It.IsAny<EmployeePhoto>()), Times.Never);
        }
    }
}
