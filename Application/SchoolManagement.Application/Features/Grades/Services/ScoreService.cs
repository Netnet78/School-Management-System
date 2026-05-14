namespace SchoolManagement.Application.Features.Grades.Services
{
    public class ScoreService : CrudServiceBase<Score>, IScoreService
    {
        public ScoreService(IScoreRepository repository) : base(repository)
        {
        }
    }
}


