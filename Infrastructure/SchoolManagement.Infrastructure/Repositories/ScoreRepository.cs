using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class ScoreRepository : BaseRepository<Score>, IScoreRepository
{
    public ScoreRepository(SchoolDbContext context) : base(context)
    {
    }
}
