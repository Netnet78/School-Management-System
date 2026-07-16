
#nullable enable
using Moq;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace Attendance_Scanner.Services.UnitTests
{
    public class AttendanceQRServiceTests
    {
        /// <summary>
        /// Verifies that the AttendanceQRService constructor does not invoke any repository methods during construction.
        /// Input conditions: Three valid mocked repository instances (both Loose and Strict MockBehavior are tested).
        /// Expected result: Construction succeeds (no exception) and no repository methods are called during construction.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AttendanceQRService_Constructor_WithValidDependencies_DoesNotCallRepositoryMethods(bool useStrictBehavior)
        {
            // Arrange
            var behavior = useStrictBehavior ? MockBehavior.Strict : MockBehavior.Loose;
            var mockStudentQRRepository = new Mock<IStudentQRRepository>(behavior);
            var mockAttendanceRepository = new Mock<IAttendanceRepository>(behavior);
            var mockStudentClassRepository = new Mock<IStudentClassRepository>(behavior);

            // Act
            var service = new AttendanceQRService(mockStudentQRRepository.Object, mockAttendanceRepository.Object, mockStudentClassRepository.Object);

            // Assert
            Assert.NotNull(service);
            // Ensure constructor itself didn't call into any repository members.
            mockStudentQRRepository.VerifyNoOtherCalls();
            mockAttendanceRepository.VerifyNoOtherCalls();
            mockStudentClassRepository.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Ensures multiple instances of AttendanceQRService constructed with different repository instances do not cross-invoke each other's mocks.
        /// Input conditions: Two independent sets of mocked repositories (Loose behavior).
        /// Expected result: Both instances are constructed successfully and no repository methods are invoked as part of construction.
        /// </summary>
        [Fact]
        public void AttendanceQRService_Constructor_MultipleInstances_IndependentMocks_NoCrossInvocation()
        {
            // Arrange
            var mockSqrA = new Mock<IStudentQRRepository>(MockBehavior.Loose);
            var mockAtrA = new Mock<IAttendanceRepository>(MockBehavior.Loose);
            var mockScrA = new Mock<IStudentClassRepository>(MockBehavior.Loose);

            var mockSqrB = new Mock<IStudentQRRepository>(MockBehavior.Loose);
            var mockAtrB = new Mock<IAttendanceRepository>(MockBehavior.Loose);
            var mockScrB = new Mock<IStudentClassRepository>(MockBehavior.Loose);

            // Act
            var serviceA = new AttendanceQRService(mockSqrA.Object, mockAtrA.Object, mockScrA.Object);
            var serviceB = new AttendanceQRService(mockSqrB.Object, mockAtrB.Object, mockScrB.Object);

            // Assert
            Assert.NotNull(serviceA);
            Assert.NotNull(serviceB);

            // Verify no calls were made to any mocks during construction (ensures isolation).
            mockSqrA.VerifyNoOtherCalls();
            mockAtrA.VerifyNoOtherCalls();
            mockScrA.VerifyNoOtherCalls();

            mockSqrB.VerifyNoOtherCalls();
            mockAtrB.VerifyNoOtherCalls();
            mockScrB.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Scaffold for a successful/happy-path QR code lookup.
        /// Purpose: Demonstrate how to set up repository mocks to return a StudentQRResponse and assert success.
        /// Input conditions: a typical valid QR code string.
        /// Expected result: TBD (commonly a StudentQRResponse with a non-null Student and Success=true).
        /// This test is skipped until the implementation contract and repository method names/signatures are confirmed.
        /// </summary>
        [Fact]
        public async Task GetStudentByQRCode_ValidCode_ReturnsExpectedResponse_Skipped()
        {
            // Arrange
            var validCode = "VALID_QR_12345";

            var student = new Student { Id = 999 };
            var studentQr = new StudentQR { Id = 10, Student = student, IsActive = true, QRCodeValue = validCode };

            var latestClass = new StudentClass
            {
                Id = 77,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                StudentId = student.Id
            };

            var mockStudentQrRepo = new Mock<IStudentQRRepository>();
            mockStudentQrRepo.Setup(r => r.GetByQRValueAsync(It.Is<string>(s => s == validCode)))
                             .ReturnsAsync(studentQr);

            var mockAttendanceRepo = new Mock<IAttendanceRepository>();
            // No prior attendances today
            mockAttendanceRepo.Setup(r => r.GetAllFromStudentId(It.Is<int>(i => i == student.Id)))
                              .ReturnsAsync(new List<Attendance>());

            Attendance? capturedAttendance = null;
            mockAttendanceRepo.Setup(r => r.AddAsync(It.IsAny<Attendance>()))
                              .Callback<Attendance>(a => capturedAttendance = a)
                              .Returns(Task.CompletedTask);

            var mockStudentClassRepo = new Mock<IStudentClassRepository>();
            mockStudentClassRepo.Setup(r => r.GetAllFromStudentIdAsync(It.Is<int>(i => i == student.Id)))
                                .ReturnsAsync(new List<StudentClass> { latestClass });

            var service = new AttendanceQRService(
                mockStudentQrRepo.Object,
                mockAttendanceRepo.Object,
                mockStudentClassRepo.Object);

            // Act
            var response = await service.GetStudentByQRCode(validCode);

            // Assert
            Assert.Equal(ReturnStatus.Success, response.Status);
            Assert.Equal(student, response.Student);
            Assert.Equal(string.Empty, response.Message);

            mockAttendanceRepo.Verify(r => r.AddAsync(It.IsAny<Attendance>()), Times.Once);
            Assert.NotNull(capturedAttendance);
            Assert.Equal(latestClass.Id, capturedAttendance!.StudentClassId);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), capturedAttendance.AttendanceDate);
            Assert.False(string.IsNullOrEmpty(capturedAttendance.OtherInfo));
        }

        /// <summary>
        /// Verifies that an empty QR code returns a Failed response with the expected message and no student.
        /// Input: empty string.
        /// Expected: ReturnStatus.Failed, message "QR Code cannot be null or empty!", Student == null.
        /// </summary>
        [Fact]
        public async Task GetStudentByQRCode_EmptyCode_ReturnsFailed()
        {
            // Arrange
            var mockSqr = new Mock<IStudentQRRepository>();
            var mockAtr = new Mock<IAttendanceRepository>();
            var mockScr = new Mock<IStudentClassRepository>();

            var svc = new AttendanceQRService(mockSqr.Object, mockAtr.Object, mockScr.Object);

            // Act
            StudentQRResponse result = await svc.GetStudentByQRCode(string.Empty);

            // Assert
            Assert.Equal(ReturnStatus.Failed, result.Status);
            Assert.Equal("QR Code cannot be null or empty!", result.Message);
            Assert.Null(result.Student);
        }

        /// <summary>
        /// Verifies that when a StudentQR is found but IsActive == false, the service returns a Failed status and the inactive message.
        /// Input: valid non-empty code with StudentQR.IsActive == false.
        /// Expected: ReturnStatus.Failed, Student == null, message contains expected Khmer text about being deactivated.
        /// </summary>
        [Fact]
        public async Task GetStudentByQRCode_InactiveStudentQR_ReturnsFailed()
        {
            // Arrange
            var student = new Student { Id = 42 };
            var studentQr = new StudentQR { Id = 1, Student = student, IsActive = false, QRCodeValue = "code" };

            var mockSqr = new Mock<IStudentQRRepository>();
            mockSqr.Setup(r => r.GetByQRValueAsync(It.IsAny<string>()))
                   .ReturnsAsync(studentQr);

            var mockAtr = new Mock<IAttendanceRepository>();
            var mockScr = new Mock<IStudentClassRepository>();

            var svc = new AttendanceQRService(mockSqr.Object, mockAtr.Object, mockScr.Object);

            // Act
            StudentQRResponse result = await svc.GetStudentByQRCode("code");

            // Assert
            Assert.Equal(ReturnStatus.Failed, result.Status);
            Assert.Null(result.Student);
            Assert.Contains("ទិន្នន័យ QR នៃកាតមួយនេះត្រូវបានបិទ", result.Message);
        }

        /// <summary>
        /// Verifies that if the latest attendance for the student is today, scanning again returns a Failed duplicate message.
        /// Input: student with a latest Attendance whose AttendanceDate equals today.
        /// Expected: ReturnStatus.Failed, message indicates cannot scan more than twice (duplicate message), Student == null.
        /// </summary>
        [Fact]
        public async Task GetStudentByQRCode_AlreadyScannedToday_ReturnsFailed()
        {
            // Arrange
            var student = new Student { Id = 7 };
            var studentQr = new StudentQR { Id = 2, Student = student, IsActive = true, QRCodeValue = "abc" };

            var todayAttendance = new Attendance
            {
                Id = 10,
                AttendanceDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ScanTime = TimeOnly.FromTimeSpan(new TimeSpan(8, 0, 0)),
                StudentClassId = 1,
                Status = AttendanceStatus.Present
            };

            var mockSqr = new Mock<IStudentQRRepository>();
            mockSqr.Setup(r => r.GetByQRValueAsync(It.IsAny<string>()))
                   .ReturnsAsync(studentQr);

            var mockAtr = new Mock<IAttendanceRepository>();
            mockAtr.Setup(r => r.GetAllFromStudentId(It.Is<int>(i => i == student.Id)))
                   .ReturnsAsync(new List<Attendance> { todayAttendance });

            var mockScr = new Mock<IStudentClassRepository>();

            var svc = new AttendanceQRService(mockSqr.Object, mockAtr.Object, mockScr.Object);

            // Act
            StudentQRResponse result = await svc.GetStudentByQRCode("abc");

            // Assert
            Assert.Equal(ReturnStatus.Failed, result.Status);
            Assert.Null(result.Student);
            Assert.Contains("ប្អូនមិនអាចស្កេនលើសពីពីរដងក្នុងមួយថ្ងៃបានទេ", result.Message);
        }

        /// <summary>
        /// Verifies that for a valid scan (student exists, active, no today's attendance, and there's a student class),
        /// an attendance record is added and a Success response with the student is returned.
        /// Input: valid active StudentQR, no today's attendance, student class list contains one class.
        /// Expected: AddAsync called once with attendance whose StudentClassId equals the latest class Id, and response.Status == Success with the student returned.
        /// </summary>
        [Fact]
        public async Task GetStudentByQRCode_ValidInput_AddsAttendanceAndReturnsSuccess()
        {
            // Arrange
            Student student = new() { Id = 100 };
            StudentQR studentQr = new() { Id = 3, Student = student, IsActive = true, QRCodeValue = "valid" };

            var olderAttendance = new Attendance
            {
                Id = 1,
                AttendanceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                ScanTime = TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                StudentClassId = 5,
                Status = AttendanceStatus.Present
            };

            var latestClass = new StudentClass
            {
                Id = 55,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                StudentId = student.Id
            };

            var mockSqr = new Mock<IStudentQRRepository>();
            mockSqr.Setup(r => r.GetByQRValueAsync(It.IsAny<string>()))
                   .ReturnsAsync(studentQr);

            var mockAtr = new Mock<IAttendanceRepository>();
            mockAtr.Setup(r => r.GetAllFromStudentId(It.Is<int>(i => i == student.Id)))
                   .ReturnsAsync(new List<Attendance> { olderAttendance });

            Attendance? capturedAttendance = null;
            mockAtr.Setup(r => r.AddAsync(It.IsAny<Attendance>()))
                   .Callback<Attendance>(a => capturedAttendance = a)
                   .Returns(Task.CompletedTask);

            var mockScr = new Mock<IStudentClassRepository>();
            mockScr.Setup(r => r.GetAllFromStudentIdAsync(It.Is<int>(i => i == student.Id)))
                   .ReturnsAsync(new List<StudentClass> { latestClass });

            var svc = new AttendanceQRService(mockSqr.Object, mockAtr.Object, mockScr.Object);

            // Act
            StudentQRResponse result = await svc.GetStudentByQRCode("valid");

            // Assert: response
            Assert.Equal(ReturnStatus.Success, result.Status);
            Assert.Equal(student, result.Student);
            Assert.Equal(string.Empty, result.Message);

            // Assert: AddAsync was called and attendance has the expected StudentClassId
            mockAtr.Verify(r => r.AddAsync(It.IsAny<Attendance>()), Times.Once);
            Assert.NotNull(capturedAttendance);
            Assert.Equal(latestClass.Id, capturedAttendance!.StudentClassId);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), capturedAttendance.AttendanceDate);
            Assert.False(string.IsNullOrEmpty(capturedAttendance.OtherInfo));
        }

        /// <summary>
        /// Verifies that if the StudentQR repository throws an exception, the service catches it and returns a Failed response containing the exception message.
        /// Input: repository throws new Exception(\"boom\").
        /// Expected: ReturnStatus.Failed and message contains \"boom\" and Student is null.
        /// </summary>
        [Fact]
        public async Task GetStudentByQRCode_StudentRepoThrows_ReturnsFailedWithExceptionMessage()
        {
            // Arrange
            var mockSqr = new Mock<IStudentQRRepository>();
            mockSqr.Setup(r => r.GetByQRValueAsync(It.IsAny<string>()))
                   .ThrowsAsync(new Exception("boom"));

            var mockAtr = new Mock<IAttendanceRepository>();
            var mockScr = new Mock<IStudentClassRepository>();

            var svc = new AttendanceQRService(mockSqr.Object, mockAtr.Object, mockScr.Object);

            // Act
            StudentQRResponse result = await svc.GetStudentByQRCode("anything");

            // Assert
            Assert.Equal(ReturnStatus.Failed, result.Status);
            Assert.Null(result.Student);
            Assert.Contains("boom", result.Message);
        }

        /// <summary>
        /// Verifies that if StudentClass repository returns an empty collection (causing a null latestStudentClass),
        /// the service will throw internally and ultimately return a Failed response containing an exception message.
        /// Input: valid StudentQR, no today's attendance, empty student class list.
        /// Expected: ReturnStatus.Failed and message contains a null-reference related message.
        /// </summary>
        [Fact]
        public async Task GetStudentByQRCode_MissingStudentClass_ReturnsFailedWithExceptionMessage()
        {
            // Arrange
            var student = new Student { Id = 200 };
            var studentQr = new StudentQR { Id = 4, Student = student, IsActive = true, QRCodeValue = "noclass" };

            var mockSqr = new Mock<IStudentQRRepository>();
            mockSqr.Setup(r => r.GetByQRValueAsync(It.IsAny<string>()))
                   .ReturnsAsync(studentQr);

            var mockAtr = new Mock<IAttendanceRepository>();
            mockAtr.Setup(r => r.GetAllFromStudentId(It.IsAny<int>()))
                   .ReturnsAsync(new List<Attendance>()); // no attendances

            var mockScr = new Mock<IStudentClassRepository>();
            mockScr.Setup(r => r.GetAllFromStudentIdAsync(It.IsAny<int>()))
                   .ReturnsAsync(new List<StudentClass>()); // empty => later code will fail when accessing Id

            var svc = new AttendanceQRService(mockSqr.Object, mockAtr.Object, mockScr.Object);

            // Act
            StudentQRResponse result = await svc.GetStudentByQRCode("noclass");

            // Assert
            Assert.Equal(ReturnStatus.Failed, result.Status);
            Assert.Null(result.Student);
            Assert.False(string.IsNullOrEmpty(result.Message));
            // The exact message may vary, ensure it indicates an unexpected error occurred
            Assert.Contains("An unexpected error occurred while processing the QR code", result.Message);
        }
    }
}