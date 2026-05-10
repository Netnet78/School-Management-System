using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class ScoreService : CrudServiceBase<Score>, IScoreService
    {
        public ScoreService(IScoreRepository repository) : base(repository)
        {
        }
    }
}
