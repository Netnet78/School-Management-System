using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class SkillService : CrudServiceBase<Skill>, ISkillService
    {
        public SkillService(ISkillRepository repository) : base(repository)
        {
        }
    }
}
