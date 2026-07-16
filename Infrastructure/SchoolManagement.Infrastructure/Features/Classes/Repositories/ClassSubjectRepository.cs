using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Classes.Repositories;

public class ClassSubjectRepository : BaseRepository<ClassSubject>, IClassSubjectRepository
{
    public ClassSubjectRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task SyncForClassAsync(int classId, IEnumerable<(int SubjectId, int? TeacherId)> subjects)
    {
        List<ClassSubject> existing = await Set
            .Where(cs => cs.ClassId == classId)
            .ToListAsync();

        Dictionary<int, ClassSubject> existingDict = existing.ToDictionary(e => e.SubjectId);
        HashSet<int> newIds = subjects.Select(s => s.SubjectId).ToHashSet();

        foreach (ClassSubject cs in existing)
        {
            if (!newIds.Contains(cs.SubjectId))
                Context.Remove(cs);
        }

        foreach ((int subjectId, int? teacherId) in subjects)
        {
            if (existingDict.TryGetValue(subjectId, out ClassSubject? existingCs))
            {
                existingCs.TeacherId = teacherId;
            }
            else
            {
                Context.Add(new ClassSubject
                {
                    ClassId = classId,
                    SubjectId = subjectId,
                    TeacherId = teacherId
                });
            }
        }

        await Context.SaveChangesAsync();
    }
}
