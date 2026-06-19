using SchoolManagement.Application.Features.Classes.Models;

namespace SchoolManagement.Application.Features.Classes.Services
{
    public class ClassService : CrudServiceBase<Class>, IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IAuthorizationService _authorizationService;

        public ClassService(
            IClassRepository repository,
            IStudentClassRepository studentClassRepository,
            IAuthorizationService authorizationService) : base(repository)
        {
            _classRepository = repository;
            _studentClassRepository = studentClassRepository;
            _authorizationService = authorizationService;
        }

        public async Task<ClassPermissions> GetPermissionsAsync()
        {
            User? user = _authorizationService.CurrentUser;

            return new ClassPermissions
            {
                CanView = (await _authorizationService.AuthorizeAsync(null, PermissionType.ViewClasses)).Status == Status.Success,
                CanInsert = (await _authorizationService.AuthorizeAsync(null, PermissionType.InsertClasses)).Status == Status.Success,
                CanEdit = (await _authorizationService.AuthorizeAsync(null, PermissionType.EditClasses)).Status == Status.Success,
                CanDelete = (await _authorizationService.AuthorizeAsync(null, PermissionType.DeleteClasses)).Status == Status.Success,
                CanManageDepartments = user?.IsAdmin() == true,
                CurrentUser = user,
            };
        }

        public async Task<ReturnResponse<IEnumerable<ClassStudentCountDto>>> GetStudentCountPerClassAsync(int fromYear, int toYear)
        {
            try
            {
                int currentYear = DateTime.UtcNow.Year;
                IEnumerable<ClassStudentCountDto> studentClassCount = await _studentClassRepository.GetStudentCountPerClass(fromYear, toYear);

                return new()
                {
                    Status = Status.Success,
                    Value = studentClassCount
                };
            }
            catch (Exception ex)
            {

                return new()
                {
                    Status = Status.Failed,
                    Message = $"Couldn't fetch student class count data for some reason\nERROR:\n{ex.Message}"
                };
            }
        }

        public async Task<ReturnResponse<Class?>> GetByIdWithSubjectsAsync(int id)
        {
            try
            {
                Class? cls = await _classRepository.GetByIdWithSubjectsAsync(id);
                return new()
                {
                    Status = cls is null ? Status.Rejected : Status.Success,
                    Value = cls,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not retrieve Class with subjects.\n{ex.Message}",
                };
            }
        }
    }
}


