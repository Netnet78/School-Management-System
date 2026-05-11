using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class ClassSubjectRepository : BaseRepository<ClassSubject>, IClassSubjectRepository
{
    public ClassSubjectRepository(SchoolDbContext context) : base(context)
    {
    }
}
