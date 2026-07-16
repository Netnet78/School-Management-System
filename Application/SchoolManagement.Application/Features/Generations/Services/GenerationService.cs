namespace SchoolManagement.Application.Features.Generations.Services
{
    public class GenerationService : CrudServiceBase<Generation>, IGenerationService
    {
        public GenerationService(IGenerationRepository repository) : base(repository)
        {
        }
    }
}


