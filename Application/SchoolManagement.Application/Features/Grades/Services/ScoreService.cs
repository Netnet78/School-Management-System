using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class ScoreService : CrudServiceBase<Score>, IScoreService
    {
        public ScoreService(IScoreRepository repository) : base(repository)
        {
        }
    }
}
