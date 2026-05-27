using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.Students.Services
{
    public class StudentService : CrudServiceBase<Student>, IStudentService
    {
        private readonly IStudentRepository _studentRepositoy;
        private readonly IAuthorizationService _authorizationService;

        public StudentService(IStudentRepository studentRepository,
                              IAuthorizationService authorizationService) : base(studentRepository)
        {
            _studentRepositoy = studentRepository;
            _authorizationService = authorizationService;
        }

        private async Task<bool> CanProceed(Student? student = null, OperatorMode operatorMode = OperatorMode.AND, params PermissionType[] requiredPermissions)
        {
            User? user = _authorizationService.CurrentUser;
            if (user == null) return false;

            ReturnResponse result = await _authorizationService.AuthorizeAsync(student, operatorMode, requiredPermissions);

            return result.Status == Status.Success;
        }

        public async override Task<ReturnResponse<IEnumerable<Student>>> GetAllAsync(
            int page, int? pageSize,
            IEnumerable<FilterCondition<Student>>? filters,
            IEnumerable<SortCriteria<Student>>? orderBy = null,
            params string[]? includes)
        {
            bool canProceed = await CanProceed(requiredPermissions: PermissionType.ViewStudents);

            if (!canProceed) return new()
            {
                Message = "អ្នកមិនមាន​​ការអនុញ្ញាតដើម្បីមើលសិស្សនោះទេ!",
                Status = Status.Rejected
            };

            User? user = _authorizationService.CurrentUser;

            if (user == null)
            {
                return new()
                {
                    Message = "រកមិនឃើញព័ត៌មាននៃអ្នកប្រើប្រាស់នៅក្នុងមូលដ្ឋានទិន្នន័យ។ " +
                    "សូមទាក់ទងអ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើអ្នកជឿជាក់ថាវាជាកំហុសបច្ចេកទេស!",
                    Status = Status.Rejected
                };
            }

            List<FilterCondition<Student>> options = filters?.ToList() ?? [];

            Expression<Func<Student, bool>>? extraPredicate = null;

            string? roleName = user.Role?.Name;

            switch (roleName)
            {
                case nameof(RoleType.Teacher):
                    int? teacherId = user.EmployeeId;

                    if (teacherId == null)
                    {
                        return new()
                        {
                            Message = "រកមិនឃើញព័ត៌មាននៃអ្នកប្រើប្រាស់នៅក្នុងមូលដ្ឋានទិន្នន័យ។ " +
                            "សូមទាក់ទងអ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើអ្នកជឿជាក់ថាវាជាកំហុសបច្ចេកទេស!",
                            Status = Status.Rejected,
                        };
                    }

                    extraPredicate = s => s.Classes.Any(sc => sc.Class.TeacherId == teacherId.Value);
                    break;

                case nameof(RoleType.HeadTeacher):
                    int? departmentId = user.Employee?.Department?.Id;

                    if (departmentId != null)
                    {
                        options.Add(new(
                            s => s.Candidate.Skill.Department.Id,
                            FilterOperator.Equals,
                            departmentId));
                    }
                    else
                    {
                        return new()
                        {
                            Message = "រកមិនឃើញព័ត៌មាននៃអ្នកប្រើប្រាស់នៅក្នុងមូលដ្ឋានទិន្នន័យ។ " +
                            "សូមទាក់ទងអ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើអ្នកជឿជាក់ថាវាជាកំហុសបច្ចេកទេស!",
                            Status = Status.Rejected
                        };
                    }
                    break;

                default:
                    break;
            }

            try
            {
                IEnumerable<Student> students = await _studentRepositoy.FindAsync(
                    options,
                    extraPredicate,
                    page,
                    pageSize,
                    orderBy,
                    "Candidate", "Candidate.Skill", "Candidate.Photo", "Classes");

                return new()
                {
                    Status = Status.Success,
                    Value = students,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Value = null,
                    Message = $"មានកំហុសបច្ចេកទេសក្នុងការទាញយកទិន្នន័យសិស្ស៖ \n {ex.Message}"
                };
            }
        }

        public async override Task<ReturnResponse<int>> GetAllCountAsync(
            int page, int? pageSize,
            IEnumerable<FilterCondition<Student>>? filters)
        {
            User? user = _authorizationService.CurrentUser;

            bool canProceed = await CanProceed(requiredPermissions: PermissionType.ViewStudents);

            if (!canProceed || user == null) return new()
            {
                Message = "អ្នកមិនមាន​​ការអនុញ្ញាតដើម្បីមើលសិស្សនោះទេ!",
                Status = Status.Rejected
            };

            List<FilterCondition<Student>> options = filters?.ToList() ?? [];

            Expression<Func<Student, bool>>? extraPredicate = null;

            string? roleName = user.Role?.Name;

            switch (roleName)
            {
                case nameof(RoleType.Teacher):
                    int? teacherId = user.Employee?.Id;

                    if (teacherId == null)
                    {
                        return new()
                        {
                            Message = "បុគ្គលិកមិនមានព័ត៌មានផ្នែកជំនាញណាមួយនោះទេ! សូមធ្វើការទាក់ទងទៅកាន់អ្នកគ្រប់គ្រងដើម្បីដោះស្រាយបញ្ហានេះ!\n",
                            Status = Status.Rejected,
                        };
                    }

                    options.Add(new(s => s.IsActive, FilterOperator.Equals, true));
                    extraPredicate = s => s.Classes.Any(sc => sc.Class.TeacherId == teacherId.Value);
                    break;

                case nameof(RoleType.HeadTeacher):
                    int? departmentId = user.Employee?.Department?.Id;

                    if (departmentId != null)
                    {
                        options.Add(new(s => s.IsActive, FilterOperator.Equals, true));
                        options.Add(new(
                            s => s.Candidate.Skill.Department.Id,
                            FilterOperator.Equals,
                            departmentId));
                    }
                    else
                    {
                        return new()
                        {
                            Message = "បុគ្គលិកមិនមានព័ត៌មាននៅក្នុងថ្នាក់​ណាមួយនោះទេ! សូមធ្វើការទាក់ទងទៅកាន់អ្នកគ្រប់គ្រងដើម្បីដោះស្រាយបញ្ហានេះ",
                            Status = Status.Rejected
                        };
                    }
                    break;

                default:
                    break;
            }

            try
            {
                int count = await _studentRepositoy.CountAsync(options, extraPredicate, page, pageSize);

                return new()
                {
                    Status = Status.Success,
                    Value = count,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"មានបញ្ហាបច្ចេកទេស អំឡុងពេលដែលកំពុងរាប់ចំនួនសិស្ស:\n {ex.Message}"
                };
            }
        }
    }
}
