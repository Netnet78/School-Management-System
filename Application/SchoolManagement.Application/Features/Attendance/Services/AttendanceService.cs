using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Extensions;
using SchoolManagement.Core.Shared.Models;
using System.Linq.Expressions;

namespace SchoolManagement.Application.Services
{
    public class AttendanceService : CrudServiceBase<Attendance>, IAttendanceService
    {
        private readonly IUserSessionService _userSessionService;
        public AttendanceService(IAttendanceRepository repository, IUserSessionService userSessionService) : base(repository)
        {
            _userSessionService = userSessionService;
        }

        public async Task<ReturnResponse<int>> GetLateStudentCountToday()
        {
            DateTime now = DateTime.Now;

            Expression<Func<Attendance, bool>> basePredicate =
                a => a.AttendanceDate == DateOnly.FromDateTime(now) &&
                     a.Status == AttendanceStatus.Late;

            var predicate = TeacherPredicate().And(basePredicate);

            return await GetAllCountAsync(predicate: predicate);
        }

        public async Task<ReturnResponse<int>> GetAbsentStudentCountToday()
        {
            DateTime now = DateTime.Now;

            Expression<Func<Attendance, bool>> basePredicate =
                a => a.AttendanceDate == DateOnly.FromDateTime(now) &&
                     a.Status == AttendanceStatus.Absent;

            var predicate = TeacherPredicate().And(basePredicate);

            return await GetAllCountAsync(predicate: predicate);
        }

        public async Task<ReturnResponse<int>> GetPresentStudentCountToday()
        {
            DateTime now = DateTime.Now;

            Expression<Func<Attendance, bool>> basePredicate =
                a => a.AttendanceDate == DateOnly.FromDateTime(now) &&
                     a.Status == AttendanceStatus.Present;

            var predicate = TeacherPredicate().And(basePredicate);

            return await GetAllCountAsync(predicate: predicate);
        }

        public async Task<ReturnResponse<int>> GetExcusedStudentCountToday()
        {
            DateTime now = DateTime.Now;

            Expression<Func<Attendance, bool>> basePredicate = a => 
            a.AttendanceDate == DateOnly.FromDateTime(now) && 
            (a.Status == AttendanceStatus.Excused || a.Status == AttendanceStatus.ExcusedLate);

            var predicate = TeacherPredicate().And(basePredicate);

            ReturnResponse<int> absentCount = await GetAllCountAsync(predicate: predicate);

            return absentCount;
        }

        private Expression<Func<Attendance, bool>> TeacherPredicate()
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
