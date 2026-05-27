using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Generations.Repositories;

public class GenerationRepository : BaseRepository<Generation>, IGenerationRepository
{
    public GenerationRepository(SchoolDbContext context) : base(context)
    {
    }
}
