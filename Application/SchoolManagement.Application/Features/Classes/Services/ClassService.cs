namespace SchoolManagement.Application.Features.Classes.Services
{
    public class ClassService : CrudServiceBase<Class>, IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly IStudentClassRepository _studentClassRepository;
        public ClassService(
            IClassRepository repository,
            IStudentClassRepository studentClassRepository) : base(repository)
        {
            _classRepository = repository;
            _studentClassRepository = studentClassRepository;
        }

        public async Task<ReturnResponse<IEnumerable<ClassStudentCountDto>>> GetStudentCountPerClassAsync()
        {
            try
            {
                int currentYear = DateTime.UtcNow.Year;
                IEnumerable<ClassStudentCountDto> studentClassCount = await _studentClassRepository.GetStudentCountPerClass(DateTime.UtcNow.Year);

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

    }
}


