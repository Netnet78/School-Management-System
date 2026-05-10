using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class GenerationService : CrudServiceBase<Generation>, IGenerationService
    {
        public GenerationService(IGenerationRepository repository) : base(repository)
        {
        }
    }
}
