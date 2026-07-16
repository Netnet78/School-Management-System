namespace SchoolManagement.Application.Features.Students.Services
{
    public class StudentService : CrudServiceBase<Student>, IStudentService
    {
        private readonly ICandidateRepository _candidateRepository;

        public StudentService(IStudentRepository studentRepository,
                              ICandidateRepository candidateRepository) : base(studentRepository)
        {
            _candidateRepository = candidateRepository;
        }

        public override Task<ReturnResponse<IEnumerable<Student>>> GetAllAsync(
            int page = 1,
            int? pageSize = null,
            IEnumerable<FilterCondition<Student>>? filters = null,
            IEnumerable<SortCriteria<Student>>? orderBy = null,
            params string[]? includes)
        {
            string[]? properties = includes;
            if (!includes.Contains(nameof(Student.Candidate)))
            {
                properties = includes != null ? ["Candidate", .. includes] : ["Candidate"];
            }
            return base.GetAllAsync(page, pageSize, filters, orderBy, properties);
        }

        public async override Task<ReturnResponse> DeleteAsync(Student entity)
        {
            try
            {
                return await base.DeleteAsync(entity);
            }
            finally
            {
                await _candidateRepository.DeleteAsync(entity.CandidateId);
            }
        }
    }
}
