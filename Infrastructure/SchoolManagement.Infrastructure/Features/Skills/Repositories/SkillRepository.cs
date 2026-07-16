using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Skills.Repositories;

public class SkillRepository : BaseRepository<Skill>, ISkillRepository
{
    public SkillRepository(SchoolDbContext context) : base(context)
    {
    }
}
