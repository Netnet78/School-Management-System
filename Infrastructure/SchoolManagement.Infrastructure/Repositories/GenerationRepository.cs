using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class GenerationRepository : BaseRepository<Generation>, IGenerationRepository
{
    public GenerationRepository(SchoolDbContext context) : base(context)
    {
    }
}
