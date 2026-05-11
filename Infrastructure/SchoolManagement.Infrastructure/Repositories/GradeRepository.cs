using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class GradeRepository : BaseRepository<Grade>, IGradeRepository
{
    public GradeRepository(SchoolDbContext context) : base(context)
    {
    }
}
