using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.Students.Services
{
    public class StudentService : CrudServiceBase<Student>, IStudentService
    {
        private readonly IStudentRepository _studentRepositoy;
        private readonly IUserSessionService _userSessionService;
        private readonly IAuthorizationService _authorizationService;

        public StudentService(IStudentRepository studentRepository,
                              IUserSessionService userSessionService,
                              IAuthorizationService authorizationService) : base(studentRepository)
        {
            _studentRepositoy = studentRepository;
            _userSessionService = userSessionService;
            _authorizationService = authorizationService;
        }

        private async Task<bool> CanProceed(Student? student = null, OperatorMode operatorMode = OperatorMode.AND, params PermissionType[] requiredPermissions)
        {
            User? user = _userSessionService.CurrentUser;
            if (user == null) return false;

            ReturnResponse result = await _authorizationService.AuthorizeAsync(student, operatorMode, requiredPermissions);

            return result.Status == Status.Success;
        }

        public async Task<ReturnResponse> DeleteStudentAsync(Student student)
        {
            Student? deleting = await _studentRepositoy.GetByIdAsync(student.Id);
            bool canProceed = await CanProceed(deleting, requiredPermissions: PermissionType.DeleteStudents);

            if (deleting == null)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"មិនអាចរកឃើញសិស្សលេខ ID៖ {student.Id} ។",
                };
            }

            if (!canProceed)
            {
                return new()
                {
                    Message = "អ្នកមិនមាន​​ការអនុញ្ញាត​ដើម្បីលុបយកសិស្សនេះ!",
                    Status = Status.Rejected
                };
            }

            try
            {
                await _studentRepositoy.DeleteAsync(deleting.Id);
                return new()
                {
                    Status = Status.Success
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"ការលុបព័ត៌មានសិស្សលេខ ID៖ {student.Id} ប្រកបដោយ​បរាជ័យ\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse<IEnumerable<Student>>> GetStudentsAsync(int page, int pageSize, StudentFilterOptions filterOptions)
        {
            bool canProceed = await CanProceed(requiredPermissions: PermissionType.ViewStudents);

            if (!canProceed) return new()
            {
                Message = "អ្នកមិនមាន​​ការអនុញ្ញាតដើម្បីមើលសិស្សនោះទេ!",
                Status = Status.Rejected
            };

            User user = _authorizationService.CurrentUser;

            if (user == null)
            {
                return new()
                {
                    Message = "រកមិនឃើញព័ត៌មាននៃអ្នកប្រើប្រាស់នៅក្នុងមូលដ្ឋានទិន្នន័យ។ " +
                    "សូមទាក់ទងអ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើអ្នកជឿជាក់ថាវាជាកំហុសបច្ចេកទេស!",
                    Status = Status.Rejected
                };
            }

            Expression<Func<Student, bool>> options = BuildFilter(filterOptions);
            Func<IQueryable<Student>, IOrderedQueryable<Student>> orderBy = BuildOrder(filterOptions);

            switch (user.Role.Name)
            {
                case nameof(RoleType.HeadTeacher):
                    int? teacherId = user.Employee?.Id;

                    if (teacherId != null)
                    {
                        Expression<Func<Student, bool>> teacherFilter =
                            s => s.Classes
                                .Any(sc => sc.IsActive && sc.Class.TeacherId == teacherId);

                        options = options.And(teacherFilter);
                    }
                    else
                    {
                        return new()
                        {
                            Message = "រកមិនឃើញព័ត៌មាននៃអ្នកប្រើប្រាស់នៅក្នុងមូលដ្ឋានទិន្នន័យ។ " +
                            "សូមទាក់ទងអ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើអ្នកជឿជាក់ថាវាជាកំហុសបច្ចេកទេស!",
                            Status = Status.Rejected,
                        };
                    }
                    break;

                case nameof(RoleType.Teacher):
                    int? departmentId = user.Employee?.Department?.Id;

                    if (departmentId != null)
                    {
                        Expression<Func<Student, bool>> headTeacherFilter =
                            s => s.Department != null && s.Department.Id == departmentId;

                        options = options.And(headTeacherFilter);
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
                    page, 
                    pageSize, 
                    orderBy, 
                    s => s.Candidate, s => s.Candidate.Skill, s => s.Candidate.Photo, s=> s.Classes);

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
                    Message = $"មានកំហុសក្នុងការទទួលបានរាយនាមសិស្ស៖ \n {ex.Message}"
                };
            }
        }

        public async Task<ReturnResponse> InsertStudentAsync(Student student)
        {
            bool canProceed = await CanProceed(requiredPermissions: PermissionType.InsertStudents);

            if (!canProceed) return new()
            {
                Message = "អ្នកមិនមានលិខិតឆ្លងដែនដើម្បីបង្កើតសិស្សថ្មី!",
                Status = Status.Rejected
            };

            try
            {
                await _studentRepositoy.AddAsync(student);
                return new()
                {
                    Status = Status.Success
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"មានកំហុសក្នុងការបង្កើតសិស្ស\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse> UpdateStudentAsync(Student student)
        {

            Student? updating = await _studentRepositoy.GetByIdAsync(student.Id);

            if (updating == null) return new()
            {
                Status = Status.Failed,
                Message = $"?????????????????? ID: {student.Id} ?????",
            };

            bool canProceed = await CanProceed(updating, requiredPermissions: PermissionType.EditStudents);

            if (!canProceed)
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "???????????????????????????????????????????!"
                };
            }

            try
            {
                await _studentRepositoy.UpdateAsync(student);
                return new()
                {
                    Status = Status.Success,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"????????????????????????????????? ????????\n{ex.Message}"
                };
            }
        }
        public async Task<ReturnResponse<int>> GetStudentsCount(int page, int pageSize, StudentFilterOptions filterOptions)
        {
            User? user = _userSessionService.CurrentUser;

            if (user == null) return new()
            {
                Status = Status.Rejected,
                Message = "????????????????????????????????? ???????????????????????????????????????????!",
            };

            bool canProceed = await CanProceed(requiredPermissions: PermissionType.ViewStudents);

            if (!canProceed) return new()
            {
                Message = "????????????????????????????????????????????????????????????!",
                Status = Status.Rejected
            };

            var options = BuildFilter(filterOptions);
            var orderBy = BuildOrder(filterOptions);

            switch (user.Role.Name)
            {
                case nameof(RoleType.HeadTeacher):
                    int? teacherId = user.Employee?.Id;

                    if (teacherId != null)
                    {
                        Expression<Func<Student, bool>> teacherFilter =
                            s => s.Classes
                                .Any(sc => sc.IsActive && sc.Class.TeacherId == teacherId);

                        options = options.And(teacherFilter);
                    }
                    else
                    {
                        return new()
                        {
                            Message = "????????????????????????????????????????????? ??????????????????????\n" +
                            "??????????????????????????????????????????? ???????????????????????????",
                            Status = Status.Rejected,
                        };
                    }
                    break;

                case nameof(RoleType.Teacher):
                    int? departmentId = user.Employee?.Department?.Id;

                    if (departmentId != null)
                    {
                        Expression<Func<Student, bool>> headTeacherFilter =
                            s => s.Department != null && s.Department.Id == departmentId;

                        options = options.And(headTeacherFilter);
                    }
                    else
                    {
                        return new()
                        {
                            Message = "????????????????????????????????????????????? ??????????????????????\n" +
                            "??????????????????????????????????????????? ???????????????????????????",
                            Status = Status.Rejected
                        };
                    }

                    break;
                default:
                    break;
            }

            try
            {
                int count = await _studentRepositoy.CountAsync(options, page, pageSize, orderBy);

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
                    Message = $"????????????????????????????? ???????:\n {ex.Message}"
                };
            }
        }

        private static Expression<Func<Student, bool>> BuildFilter(StudentFilterOptions options)
        {
            return student =>
                (string.IsNullOrEmpty(options.Search)
                    || student.Candidate.FullName.Contains(options.Search))
                &&
                (!options.Gender.HasValue || student.Candidate.Gender == options.Gender)
                &&
                (!options.FromDate.HasValue || student.CreatedAt >= options.FromDate)
                &&
                (!options.ToDate.HasValue || student.CreatedAt <= options.ToDate)
                &&
                (!options.IsActive.HasValue || (student.Candidate.Skill.IsActive && student.IsActive) == options.IsActive)
                &&
                (!options.StayType.HasValue || student.Candidate.StayType == options.StayType);
        }

        private static Func<IQueryable<Student>, IOrderedQueryable<Student>> BuildOrder(StudentFilterOptions options)
        {
            if (!Enum.TryParse(typeof(StudentField), options.SortBy, true, out object? sortField))
            {
                sortField = StudentField.Id;
            }

            return query =>
            {
                return (StudentField)sortField switch
                {
                    StudentField.FullName => options.OrderBy == OrderType.Ascending
                        ? query.OrderBy(x => x.Candidate.LastName).ThenBy(x => x.Candidate.FirstName).ThenBy(x => x.Candidate.Id)
                        : query.OrderByDescending(x => x.Candidate.LastName).ThenByDescending(x => x.Candidate.FirstName).ThenByDescending(x => x.Candidate.Id),

                    StudentField.LatinFullName => options.OrderBy == OrderType.Ascending
                        ? query.OrderBy(x => x.Candidate.LatinLastName).ThenBy(x => x.Candidate.LatinFirstName).ThenBy(x => x.Candidate.Id)
                        : query.OrderByDescending(x => x.Candidate.LatinLastName).ThenByDescending(x => x.Candidate.LatinFirstName).ThenByDescending(x => x.Candidate.Id),

                    StudentField.CreatedAt => options.OrderBy == OrderType.Ascending
                        ? query.OrderBy(x => x.Candidate.CreatedAt).ThenBy(x => x.Candidate.Id)
                        : query.OrderByDescending(x => x.Candidate.CreatedAt).ThenByDescending(x => x.Candidate.Id),

                    StudentField.DateOfBirth => options.OrderBy == OrderType.Ascending
                        ? query.OrderBy(x => x.Candidate.DateOfBirth).ThenBy(x => x.Candidate.Id)
                        : query.OrderByDescending(x => x.Candidate.DateOfBirth).ThenByDescending(x => x.Candidate.Id),

                    StudentField.Skill => options.OrderBy == OrderType.Ascending
                        ? query.OrderBy(x => x.Candidate.Skill.KhmerName).ThenBy(x => x.Candidate.Id)
                        : query.OrderByDescending(x => x.Candidate.Skill.KhmerName).ThenByDescending(x => x.Candidate.Id),

                    StudentField.Gender => options.OrderBy == OrderType.Ascending
                        ? query.OrderBy(x => x.Candidate.Gender).ThenBy(x => x.Candidate.Id)
                        : query.OrderByDescending(x => x.Candidate.Gender).ThenByDescending(x => x.Candidate.Id),

                    _ => options.OrderBy == OrderType.Ascending
                        ? query.OrderBy(x => x.Candidate.Id)
                        : query.OrderByDescending(x => x.Candidate.Id),
                };
            };
        }

    }
}

