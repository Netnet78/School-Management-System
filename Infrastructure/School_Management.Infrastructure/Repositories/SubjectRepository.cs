using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories;

public class SubjectRepository : BaseRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(SchoolDbContext context) : base(context)
    {
    }
}
