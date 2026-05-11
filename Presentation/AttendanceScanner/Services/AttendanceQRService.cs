using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Time;


namespace AttendanceScanner.Services
{
    public interface IAttendanceQRService
    {
        Task<StudentQRResponse> GetStudentByQRCode(string code);
    }

    public class AttendanceQRService : IAttendanceQRService
    {
        private readonly IStudentQRRepository _studentQRRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IStudentClassRepository _studentClassRepository;

        public AttendanceQRService(IStudentQRRepository sqr, IAttendanceRepository atr, IStudentClassRepository scr)
        {
            _studentQRRepository = sqr;
            _attendanceRepository = atr;
            _studentClassRepository = scr;
        }

        public async Task<StudentQRResponse> GetStudentByQRCode(string code)
        {
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                DateTime cambodiaNow = utcNow.ToLocalTimeZone();

                TimeSpan scanTime = cambodiaNow.TimeOfDay;
                TimeSpan startTime = new(5, 0, 0);
                TimeSpan endTime = new(17, 0, 0);

                if ((scanTime >= startTime && scanTime < endTime) == false)
                {
                    return new()
                    {
                        Status = Status.Failed,
                        Message = "ប្អូនមិនអាចស្កេនវត្តមាននៅពេលវេលាម៉ោងនេះទេ! សូមព្យាយាមនៅពេលក្រោយ!",
                        Student = null
                    };
                }

                if (string.IsNullOrEmpty(code))
                {
                    return new()
                    {
                        Status = Status.Failed,
                        Message = "QR Code cannot be null or empty!",
                        Student = null
                    };
                }

                StudentQR? studentQR = await _studentQRRepository.GetByQRValueAsync(code);

                if (studentQR == null)
                {
                    return new()
                    {
                        Status = Status.Failed,
                        Message = "គ្មាន​ទិន្នន័យសិស្សនៃ QR Code មួយនេះទេ! សូមព្យាយាមម្ដងទៀតនៅពេលក្រោយ!",
                        Student = null
                    };
                }

                if (studentQR.IsActive == false)
                {
                    return new()
                    {
                        Status = Status.Failed,
                        Message = "ទិន្នន័យ QR នៃកាតមួយនេះត្រូវបានបិទ! ប្រសិនបើប្អូនគិតថា វាជាកំហុសបច្ចេកទេស, សូមប្អូនជូនដំណឹងទៅកាន់លោកគ្រូអ្នកគ្រូភ្លាមៗ!",
                        Student = null
                    };
                }

                IEnumerable<Attendance> studentAttendances = await _attendanceRepository.GetAllFromStudentId(studentQR.Student.Id);
                Attendance? latestAttendance = studentAttendances.OrderByDescending(sa => new DateTime(sa.AttendanceDate, sa.ScanTime)).FirstOrDefault();

                if (latestAttendance != null && DateOnly.FromDateTime(cambodiaNow) == latestAttendance.AttendanceDate)
                {
                    return new()
                    {
                        Status = Status.Failed,
                        Message = "ប្អូនមិនអាចស្កេនលើសពីពីរដងក្នុងមួយថ្ងៃបានទេ!",
                        Student = null
                    };
                }

                AttendanceStatus attendanceStatus = AttendanceStatus.Present;

                TimeSpan lateTime = new(7, 30, 0);
                TimeSpan tooLateTime = new(11, 30, 0);

                if (scanTime > lateTime)
                {
                    attendanceStatus = AttendanceStatus.Late;
                }
                if (scanTime > tooLateTime)
                {
                    attendanceStatus = AttendanceStatus.Absent;
                }

                StudentClass latestStudentClass = (await _studentClassRepository.GetAllFromStudentIdAsync(studentQR!.Student.Id))!.OrderByDescending(sc => sc.EndDate).FirstOrDefault()!;

                Attendance attendance = new()
                {
                    StudentClassId = latestStudentClass.Id,
                    AttendanceDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ScanTime = TimeOnly.FromTimeSpan(DateTime.UtcNow.TimeOfDay),
                    MarkedByEmployeeId = null,
                    Status = attendanceStatus,
                    OtherInfo = "This attendance was auto-marked by the Attendance Management System",
                };
                await _attendanceRepository.AddAsync(attendance);

                return new()
                {
                    Status = Status.Success,
                    Message = string.Empty,
                    Student = studentQR.Student
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"An unexpected error occurred while processing the QR code.\n{ex.Message}",
                    Student = null
                };
            }
        }
    }
}
