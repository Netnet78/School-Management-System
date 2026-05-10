using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class SkillService : CrudServiceBase<Skill>, ISkillService
    {
        public SkillService(ISkillRepository repository) : base(repository)
        {
        }
    }
}
