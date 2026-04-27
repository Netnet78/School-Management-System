using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;


namespace Attendance_Scanner.Services
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
                //bool disableTimeCheck = true;
                TimeSpan scanTime = DateTime.Now.TimeOfDay;
                TimeSpan startTime = new(5, 0, 0);
                TimeSpan endTime = new(17, 0, 0);

                if ((scanTime >= startTime && scanTime < endTime) == false)
                {
                    return new()
                    {
                        Status = ReturnStatus.Failed,
                        Message = "ប្អូនមិនអាចស្កេនវត្តមាននៅពេលវេលាម៉ោងនេះទេ! សូមព្យាយាមនៅពេលក្រោយ!",
                        Student = null
                    };
                }

                if (string.IsNullOrEmpty(code))
                {
                    return new()
                    {
                        Status = ReturnStatus.Failed,
                        Message = "QR Code cannot be null or empty!",
                        Student = null
                    };
                }

                StudentQR? studentQR = await _studentQRRepository.GetByQRValueAsync(code);

                if (studentQR == null)
                {
                    return new()
                    {
                        Status = ReturnStatus.Failed,
                        Message = "គ្មាន​ទិន្នន័យសិស្សនៃ QR Code មួយនេះទេ! សូមព្យាយាមម្ដងទៀតនៅពេលក្រោយ!",
                        Student = null
                    };
                }

                if (studentQR.IsActive == false)
                {
                    return new()
                    {
                        Status = ReturnStatus.Failed,
                        Message = "ទិន្នន័យ QR នៃកាតមួយនេះត្រូវបានបិទ! ប្រសិនបើប្អូនគិតថា វាជាកំហុសបច្ចេកទេស, សូមប្អូនជូនដំណឹងទៅកាន់លោកគ្រូអ្នកគ្រូភ្លាមៗ!",
                        Student = null
                    };
                }

                IEnumerable<Attendance> studentAttendances = await _attendanceRepository.GetAllFromStudentId(studentQR.Student.Id);
                Attendance? latestAttendance = studentAttendances.OrderByDescending(sa => new DateTime(sa.AttendanceDate, sa.ScanTime)).FirstOrDefault();

                if (latestAttendance != null && DateOnly.FromDateTime(DateTime.Now) == latestAttendance.AttendanceDate)
                {
                    return new()
                    {
                        Status = ReturnStatus.Failed,
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
                    AttendanceDate = DateOnly.FromDateTime(DateTime.Now),
                    ScanTime = TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay),
                    MarkedByEmployeeId = null,
                    Status = attendanceStatus,
                    OtherInfo = "This attendance was auto-marked by the Attendance Management System",
                };
                await _attendanceRepository.AddAsync(attendance);

                return new()
                {
                    Status = ReturnStatus.Success,
                    Message = string.Empty,
                    Student = studentQR.Student
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = ReturnStatus.Failed,
                    Message = $"An unexpected error occurred while processing the QR code.\n{ex.Message}",
                    Student = null
                };
            }
        }
    }
}
