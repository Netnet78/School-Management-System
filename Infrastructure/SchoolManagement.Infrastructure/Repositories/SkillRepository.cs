using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class SkillRepository : BaseRepository<Skill>, ISkillRepository
{
    public SkillRepository(SchoolDbContext context) : base(context)
    {
    }
}
