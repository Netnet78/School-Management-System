using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories;

public class GenerationRepository : BaseRepository<Generation>, IGenerationRepository
{
    public GenerationRepository(SchoolDbContext context) : base(context)
    {
    }
}
