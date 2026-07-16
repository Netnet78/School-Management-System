using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Time;
using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.Attendances.Services
{
    public class AttendanceService : CrudServiceBase<Attendance>, IAttendanceService
    {
        private readonly IAttendanceRepository _repository;
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IUserSessionService _userSessionService;
        public AttendanceService(
            IAttendanceRepository repository,
            IUserSessionService userSessionService,
            IStudentClassRepository studentClassRepository) : base(repository)
        {
            _repository = repository;
            _userSessionService = userSessionService;
            _studentClassRepository = studentClassRepository;
        }

        public async Task<ReturnResponse<int>> GetLateStudentCountToday()
        {
            return await GetCountTodayAsync(AttendanceStatus.Late);
        }

        public async Task<ReturnResponse<int>> GetAbsentStudentCountToday()
        {
            return await GetCountTodayAsync(AttendanceStatus.Absent);
        }

        public async Task<ReturnResponse<int>> GetPresentStudentCountToday()
        {
            return await GetCountTodayAsync(AttendanceStatus.Present);
        }

        public async Task<ReturnResponse<int>> GetExcusedStudentCountToday()
        {
            return await GetCountTodayAsync(AttendanceStatus.Excused, AttendanceStatus.ExcusedLate);
        }

        private async Task<ReturnResponse<int>> GetCountTodayAsync(params AttendanceStatus[] statuses)
        {
            DateTime startOfToday = DateTime.Today.ToUtcTimeZone();
            DateTime startOfTomorrow = startOfToday.AddDays(1);

            FilterCondition<Attendance>[] filters =
            [
                new(x => x.AttendanceDateTime, FilterOperator.GreaterThanOrEqual, startOfToday),
                new(x => x.AttendanceDateTime, FilterOperator.LessThan, startOfTomorrow),
                new(x => x.Status, FilterOperator.In, statuses.Cast<object>()),

            ];

            IEnumerable<Attendance> records =
                await _repository.FindAsync(
                    filters: filters,
                    includes: ["StudentClass.Class.Generation.Department"]);

            int count = records.Count(TeacherPredicate().Compile());

            return new()
            {
                Status = Status.Success,
                Value = count
            };
        }


        private Expression<Func<Attendance, bool>> TeacherPredicate()
        {
            User? user = _userSessionService.CurrentUser ?? 
                throw new Exception("User session cannot be null!");

            if (user.IsValidRole(RoleType.Admin))
                return a => true;

            return user.Role.Name switch
            {
                nameof(RoleType.HeadTeacher) => a => user.Employee != null &&
                                    user.Employee.DepartmentId == a.StudentClass.Class.Department.Id,
                nameof(RoleType.Teacher) => a => user.Employee != null &&
                                    user.EmployeeId == a.StudentClass.Class.TeacherId,
                _ => a => false,
            };

        }

        public async Task<ReturnResponse<IEnumerable<AttendanceDTO>>> GetClassMonthlyAttendance(
            int classId,
            int month,
            int year)
        {
            if (classId <= 0)
                throw new InvalidOperationException("Invalid class ID");

            if (month <= 0 || month > 12)
                throw new InvalidOperationException("Invalid month");

            var studentClasses = await _studentClassRepository.FindAsync(
            [
                new(x => x.ClassId, FilterOperator.Equals, classId)
            ],
            includes: ["Student"]);


            var studentClassIds = studentClasses
                .Select(x => x.Id)
                .ToList();


            var attendanceRecords = await _repository.FindAsync(
            [
                new(x => studentClassIds.Contains(x.StudentClassId)),
                new(x => x.AttendanceDateTime.Month == month),
                new(x => x.AttendanceDateTime.Year == year)
            ]);

            IEnumerable<AttendanceDTO> summaries = studentClasses.Select(studentClass =>
            {
                var records = attendanceRecords
                    .Where(x => x.StudentClassId == studentClass.Id);

                Dictionary<AttendanceDay, AttendanceStatus> daily = records
                    .GroupBy(x => new AttendanceDay(x.AttendanceDateTime.Day, x.AttendanceDateTime.DayOfWeek.ToString()))
                    .ToDictionary(
                        x => x.Key,
                        x => x.First().Status
                    );

                return new AttendanceDTO
                {
                    StudentId = studentClass.StudentId,
                    StudentName = studentClass.Student.FullName,
                    DailyAttendance = daily,

                    PresentCount = records.Count(r =>
                        r.Status == AttendanceStatus.Present),

                    LateCount = records.Count(r =>
                        r.Status == AttendanceStatus.Late ||
                        r.Status == AttendanceStatus.ExcusedLate),

                    AbsentCount = records.Count(r =>
                        r.Status == AttendanceStatus.Absent),

                    HalfDayCount = records.Count(r =>
                        r.Status == AttendanceStatus.EarlyLeave),

                    ExcusedCount = records.Count(r =>
                        r.Status == AttendanceStatus.Excused)
                };
            });


            return new()
            {
                Status = Status.Success,
                Value = summaries,
            };
        }
        public async Task<ReturnResponse<IEnumerable<Attendance>>> GetAttendancesWithCursorAsync(IEnumerable<FilterCondition<Attendance>>? filters, DateTime? lastDate, int? lastId, int pageSize, params string[] includes)
        {
            try
            {
                var entities = await _repository.GetAttendancesWithCursorAsync(filters, lastDate, lastId, pageSize, includes);
                return new()
                {
                    Status = Status.Success,
                    Value = entities
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not retrieve Attendances with cursor.\n{ex.Message}"
                };
            }
        }
    }
}
