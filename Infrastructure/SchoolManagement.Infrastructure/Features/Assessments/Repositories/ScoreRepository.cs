using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Assessments.Repositories
{
    public class ScoreRepository : BaseRepository<Score>, IScoreRepository
    {
        public ScoreRepository(SchoolDbContext context) : base(context)
        {

        }
    }
}
