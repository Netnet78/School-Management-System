using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Assessments.Contracts
{
    public interface IAssessmentRepository : IBaseRepository<Assessment>
    {
        Task UpsertRangeAsync(int examId, int classSubjectId, IEnumerable<(int StudentClassId, decimal TotalScore)> entries);
    }
}
