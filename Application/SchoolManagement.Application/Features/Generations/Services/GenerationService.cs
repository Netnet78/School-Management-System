using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class GenerationService : CrudServiceBase<Generation>, IGenerationService
    {
        public GenerationService(IGenerationRepository repository) : base(repository)
        {
        }
    }
}
