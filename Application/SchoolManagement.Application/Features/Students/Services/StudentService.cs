using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Application.DTOs;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using System.Linq.Expressions;
using SchoolManagement.Core.Shared.Models;
using SchoolManagement.Core.Shared.Extensions;

namespace SchoolManagement.Application.Services
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
                    Message = $"មិនអាចរកឃើញសិស្សដែលមាន ID: {student.Id} នេះទេ",
                };
            }

            if (!canProceed)
            {
                return new()
                {
                    Message = "អ្នកគ្មានសិទិ្ធ​លុបទិន្នន័យសិស្សនេះឡើយ!",
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
                    Message = $"មិនអាចលុបសិស្សដែលមាន ID: {student.Id} នេះបានទេ\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse<IEnumerable<Student>>> GetStudentsAsync(int page, int pageSize, StudentFilterOptions filterOptions)
        {
            bool canProceed = await CanProceed(requiredPermissions: PermissionType.ViewStudents);

            if (!canProceed) return new()
            {
                Message = "អ្នកមិនអាចអានទិន្នន័យសិស្សទាំងអស់នេះបានទេ!",
                Status = Status.Rejected
            };

            User? user = _userSessionService.CurrentUser;

            if (user == null)
            {
                return new()
                {
                    Message = "អ្នកគ្មានសិទ្ធិទាញយកទិន្នន័យបានទេ ប្រសិនបើអ្នកមិនមែនជាអ្នកប្រើប្រាស់ពិតប្រាកដ!",
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
                            Message = "ទិន្នន័យអ្នកប្រើប្រាស់ដែលកំពុងប្រើកម្មវិធីនេះ មិនមាននៅក្នុងផ្នែកណាទេ\n" +
                            "សូមធ្វើការជូនដំណឹងទៅកាន់អ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើវាជាកំហុសបច្ចេកទេស។",
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
                            Message = "ទិន្នន័យអ្នកប្រើប្រាស់ដែលកំពុងប្រើកម្មវិធីនេះ មិនមាននៅក្នុងផ្នែកណាទេ\n" +
                            "សូមធ្វើការជូនដំណឹងទៅកាន់អ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើវាជាកំហុសបច្ចេកទេស។",
                            Status = Status.Rejected
                        };
                    }

                    break;
                default:
                    break;
            }

            try
            {
                IEnumerable<Student> students = await _studentRepositoy.FindAsync(options, page, pageSize, orderBy, s => s.Candidate, s => s.Candidate.Skill, s => s.Candidate.Photo, s=> s.Classes);

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
                    Message = $"មិនអាចទាញរកទិន្នន័យសិស្សបានទេ មូលហេតុ:\n {ex.Message}"
                };
            }
        }

        public async Task<ReturnResponse> InsertStudentAsync(Student student)
        {
            bool canProceed = await CanProceed(requiredPermissions: PermissionType.InsertStudents);

            if (!canProceed) return new()
            {
                Message = "អ្នកគ្មានសិទ្ធិ​បញ្ចូល​ទិន្នន័យសិស្សនេះបានទេ!",
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
                    Message = $"មិនអាចបន្ថែមសិស្សបានទេ មូលហេតុ៖\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse> UpdateStudentAsync(Student student)
        {

            Student? updating = await _studentRepositoy.GetByIdAsync(student.Id);

            if (updating == null) return new()
            {
                Status = Status.Failed,
                Message = $"គ្មានសិស្សណាដែលមាន ID: {student.Id} នេះទេ",
            };

            bool canProceed = await CanProceed(updating, requiredPermissions: PermissionType.EditStudents);

            if (!canProceed)
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "អ្នកគ្មានសិទ្ធិក្នុងការកែប្រែសិស្សនេះបានឡើយ!"
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
                    Message = $"មិនអាចកែប្រែទិន្នន័យសិស្សនេះបានទេ មូលហេតុ៖\n{ex.Message}"
                };
            }
        }
        public async Task<ReturnResponse<int>> GetStudentsCount(int page, int pageSize, StudentFilterOptions filterOptions)
        {
            User? user = _userSessionService.CurrentUser;

            if (user == null) return new()
            {
                Status = Status.Rejected,
                Message = "អ្នកគ្មានសិទ្ធិទាញយកទិន្នន័យបានទេ ប្រសិនបើអ្នកមិនមែនជាអ្នកប្រើប្រាស់ពិតប្រាកដ!",
            };

            bool canProceed = await CanProceed(requiredPermissions: PermissionType.ViewStudents);

            if (!canProceed) return new()
            {
                Message = "អ្នកគ្មានសិទ្ធិក្នុងការរាប់ចំនួនទិន្នន័យសិស្សទាំងអស់នេះបានទេ!",
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
                            Message = "ទិន្នន័យអ្នកប្រើប្រាស់ដែលកំពុងប្រើកម្មវិធីនេះ មិនមាននៅក្នុងផ្នែកណាទេ\n" +
                            "សូមធ្វើការជូនដំណឹងទៅកាន់អ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើវាជាកំហុសបច្ចេកទេស។",
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
                            Message = "ទិន្នន័យអ្នកប្រើប្រាស់ដែលកំពុងប្រើកម្មវិធីនេះ មិនមាននៅក្នុងផ្នែកណាទេ\n" +
                            "សូមធ្វើការជូនដំណឹងទៅកាន់អ្នកគ្រប់គ្រងភ្លាមៗ ប្រសិនបើវាជាកំហុសបច្ចេកទេស។",
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
                    Message = $"មិនអាចទាញរកទិន្នន័យសិស្សបានទេ មូលហេតុ:\n {ex.Message}"
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
