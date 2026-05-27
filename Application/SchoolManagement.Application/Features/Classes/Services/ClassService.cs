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


