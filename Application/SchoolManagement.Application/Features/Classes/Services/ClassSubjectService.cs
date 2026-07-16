namespace SchoolManagement.Application.Features.Classes.Services
{
    public class ClassSubjectService : CrudServiceBase<ClassSubject>, IClassSubjectService
    {
        private readonly IClassSubjectRepository _classSubjectRepository;

        public ClassSubjectService(IClassSubjectRepository repository) : base(repository)
        {
            _classSubjectRepository = repository;
        }

        public async Task<ReturnResponse> SyncSubjectsForClassAsync(int classId, IEnumerable<(int SubjectId, int? TeacherId)> subjects)
        {
            try
            {
                await _classSubjectRepository.SyncForClassAsync(classId, subjects);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not sync subjects for class.\n{ex.Message}"
                };
            }
        }
    }
}


