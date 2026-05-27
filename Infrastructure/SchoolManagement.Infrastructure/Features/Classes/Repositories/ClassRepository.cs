using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Classes.Repositories;

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

    public async Task<Class?> GetByIdWithSubjectsAsync(int id)
    {
        return await Set
            .Include(c => c.Grade)
            .Include(c => c.Generation)
                .ThenInclude(g => g.Department)
            .Include(c => c.Teacher)
            .Include(c => c.Subjects)
                .ThenInclude(cs => cs.Subject)
            .Include(c => c.Subjects)
                .ThenInclude(cs => cs.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
