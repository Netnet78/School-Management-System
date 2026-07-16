using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Subjects.Repositories;

public class SubjectRepository : BaseRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<Subject?> GetByIdWithMappersAsync(int id)
    {
        return await Set
            .Include(s => s.Mappers)
            .ThenInclude(m => m.Component)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SubjectComponent?> FindComponentByNameAsync(string name)
    {
        return await Context.SubjectComponents
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<Dictionary<string, SubjectComponent>> FindComponentsByNamesAsync(IEnumerable<string> names)
    {
        return await Context.SubjectComponents
            .Where(c => names.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name);
    }

    public async Task<IEnumerable<SubjectMapper>> GetMappersForSubjectAsync(int subjectId)
    {
        return await Context.SubjectMappers
            .Include(m => m.Component)
            .Where(m => m.SubjectId == subjectId)
            .ToListAsync();
    }
}
