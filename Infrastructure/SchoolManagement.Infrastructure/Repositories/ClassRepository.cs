using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class ClassRepository : BaseRepository<Class>, IClassRepository
{
    public ClassRepository(SchoolDbContext context) : base(context)
    {
    }

    protected override IQueryable<Class> CreateQuery()
    {
        return Set
            .Include(c => c.Grade)
            .Include(c => c.Generation)
                .ThenInclude(g => g.Department)
            .Include(c => c.Teacher);
    }
}
