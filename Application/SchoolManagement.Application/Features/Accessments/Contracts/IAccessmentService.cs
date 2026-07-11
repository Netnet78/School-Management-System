using SchoolManagement.Core.Features.Assessments.Models;

namespace SchoolManagement.Application.Features.Assessments.Contracts
{
    public interface IAssessmentService : ICrudService<Assessment>
    {
        Task<ReturnResponse> UpsertRangeAsync(int examId, int classSubjectId, IEnumerable<(int StudentClassId, int MapperId, int ComponentId, decimal ScoreAmount)> entries);
    }
}
