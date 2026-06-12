using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Time;
using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.Attendances.Services
{
    public class AttendanceService : CrudServiceBase<Attendance>, IAttendanceService
    {
        private readonly IAttendanceRepository _repository;
        private readonly IUserSessionService _userSessionService;
        public AttendanceService(IAttendanceRepository repository, IUserSessionService userSessionService) : base(repository)
        {
            _repository = repository;
            _userSessionService = userSessionService;
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
    }
}




