using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Accessments.Repositories;

public class ScoreRepository : BaseRepository<Assessment>, IAccessmentRepository
{
    public ScoreRepository(SchoolDbContext context) : base(context)
    {
    }
}
