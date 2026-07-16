using SchoolManagement.Core.Features.Attendances.Enums;
using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Extensions;
using SchoolManagement.Core.Shared.Time;


namespace AttendanceScanner.Services
{
    public interface IAttendanceQRService
    {
        Task<StudentQRResponse> MarkStudent(string code);
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

        public async Task<StudentQRResponse> MarkStudent(string code)
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
                        Status = Status.Rejected,
                        Message = "ប្អូនមិនអាចស្កេនវត្តមាននៅពេលវេលាម៉ោងនេះទេ! សូមព្យាយាមនៅពេលក្រោយ!",
                        Student = null
                    };
                }

                if (string.IsNullOrEmpty(code))
                {
                    return new()
                    {
                        Status = Status.Rejected,
                        Message = "QR Code cannot be null or empty!",
                        Student = null
                    };
                }

                StudentQR? studentQR = await _studentQRRepository.GetByQRValueAsync(code);

                if (studentQR == null)
                {
                    return new()
                    {
                        Status = Status.Rejected,
                        Message = "គ្មាន​ទិន្នន័យសិស្សនៅក្នុង QR Code មួយនេះទេ! សូមព្យាយាមម្ដងទៀតនៅពេលក្រោយ!",
                        Student = null
                    };
                }

                if (studentQR.IsActive == false)
                {
                    return new()
                    {
                        Status = Status.Rejected,
                        Message = "ទិន្នន័យ QR នៃកាតមួយនេះត្រូវបានបិទ! ប្រសិនបើប្អូនគិតថា វាជាកំហុសបច្ចេកទេស, សូមប្អូនជូនដំណឹងទៅកាន់លោកគ្រូអ្នកគ្រូភ្លាមៗ!",
                        Student = null
                    };
                }

                DateTime today = new(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc);

                IEnumerable<Attendance> studentAttendances = await _attendanceRepository.FindAsync(
                    [new(a => a.AttendanceDateTime, FilterOperator.GreaterThanOrEqual, today)]);

                if (studentAttendances.Any())
                {
                    return new()
                    {
                        Status = Status.Rejected,
                        Message = $"ព័ត៌មានវត្តមានរបស់ប្អូនត្រូវបានកាត់ជា \"{studentAttendances.First().Status.GetDescription()}\" " +
                        $"រួចរាល់មកហើយ! សូមប្អូនធ្វើការបន្តទៅមុខ ។",
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

                StudentClass? latestStudentClass = (await _studentClassRepository.GetAllFromStudentIdAsync(studentQR.Student.Id))?.OrderByDescending(sc => sc.EndDate).FirstOrDefault()!;

                if (latestStudentClass == null)
                {
                    return new()
                    {
                        Status = Status.Rejected,
                        Message = $"ទិន្នន័យសិស្សឈ្មោះ \"{studentQR.Student.FullName}\" គ្មានទិន្នន័យ​​ថ្នាក់រៀន" +
                        $"នៅក្នុងមូលដ្ឋានទិន្នន័យទេ! សូមព្យាយាមម្ដងទៀតនៅពេលក្រោយ!",
                    };
                }

                Attendance attendance = new()
                {
                    StudentClassId = latestStudentClass.Id,
                    AttendanceDateTime = DateTime.UtcNow,
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
