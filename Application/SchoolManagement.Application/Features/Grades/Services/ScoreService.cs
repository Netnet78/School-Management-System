using SchoolManagement.Core.Features.Assessments.Models;

namespace SchoolManagement.Application.Features.Grades.Services
{
    public class ScoreService : CrudServiceBase<Score>, IScoreService
    {
        public ScoreService(IScoreRepository repository) : base(repository)
        {
        }
    }
}


