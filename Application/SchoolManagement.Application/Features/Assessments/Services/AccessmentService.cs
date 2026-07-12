using SchoolManagement.Core.Features.Assessments.Models;

namespace SchoolManagement.Application.Features.Assessments.Services
{
    public class AssessmentService : CrudServiceBase<Assessment>, IAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;

        public AssessmentService(IAssessmentRepository repository) : base(repository)
        {
            _assessmentRepository = repository;
        }

        public async Task<ReturnResponse> UpsertRangeAsync(int examId, int classSubjectId, IEnumerable<(int StudentClassId, int MapperId, int ComponentId, decimal ScoreAmount)> entries)
        {
            try
            {
                await _assessmentRepository.UpsertRangeAsync(examId, classSubjectId, entries);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"មានកំហុសក្នុងការរក្សាទុកពិន្ទុ: {ex.Message}"
                };
            }
        }
    }
}
