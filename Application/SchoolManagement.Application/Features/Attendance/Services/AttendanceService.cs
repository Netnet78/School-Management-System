using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.Attendance.Services
{
    public class AttendanceService : CrudServiceBase<SchoolManagement.Core.Features.Attendances.Models.Attendance>, IAttendanceService
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
            DateTime now = DateTime.Now;
            DateOnly today = DateOnly.FromDateTime(now);

            SchoolManagement.Core.Shared.Models.FilterCondition<SchoolManagement.Core.Features.Attendances.Models.Attendance>[] filters =
            [
                new(a => a.AttendanceDate, FilterOperator.Equals, today)
            ];

            if (statuses.Length > 0)
            {
                filters = [
                    .. filters,
                    new(a => a.Status, FilterOperator.In, statuses.Cast<object>())
                ];
            }

            Expression<Func<SchoolManagement.Core.Features.Attendances.Models.Attendance, object?>>[] includes =
            [
                a => a.StudentClass,
                a => a.StudentClass.Class,
                a => a.StudentClass.Class.Generation,
                a => a.StudentClass.Class.Generation.Department
            ];

            IEnumerable<SchoolManagement.Core.Features.Attendances.Models.Attendance> records =
                await _repository.FindAsync(
                    filters: filters,
                    includes: includes);

            int count = records.Where(TeacherPredicate().Compile()).Count();

            return new()
            {
                Status = Status.Success,
                Value = count
            };
        }

        private Expression<Func<SchoolManagement.Core.Features.Attendances.Models.Attendance, bool>> TeacherPredicate()
        {
            User? user = _userSessionService.CurrentUser ?? 
                throw new Exception("User session cannot be null!");

            if (user.Role.Name == RoleType.Admin.ToString()) return a => true;

            return user.Role.Name switch
            {
                nameof(RoleType.HeadTeacher) => a => user.Employee != null &&
                                    user.Employee.DepartmentId == a.StudentClass.Class.Department.Id,
                nameof(RoleType.Teacher) => a => user.Employee != null &&
                                    user.EmployeeId == a.StudentClass.Class.TeacherId,
                _ => a => false,
            };

            //return attendance =>
            //    attendance.StudentClass.Class.Generation.DepartmentId == depId;
        }
    }
}




