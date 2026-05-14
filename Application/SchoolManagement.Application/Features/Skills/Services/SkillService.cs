namespace SchoolManagement.Application.Features.Skills.Services
{
    public class SkillService : CrudServiceBase<Skill>, ISkillService
    {
        public SkillService(ISkillRepository repository) : base(repository)
        {
        }
    }
}


