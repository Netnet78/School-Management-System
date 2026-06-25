using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Assessments.Repositories;

public class ScoreRepository : BaseRepository<Assessment>, IAssessmentRepository
{
    public ScoreRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task UpsertRangeAsync(int examId, int classSubjectId, IEnumerable<(int StudentClassId, decimal TotalScore)> entries)
    {
        List<(int StudentClassId, decimal TotalScore)> entryList = entries.ToList();
        if (entryList.Count == 0) return;

        List<int> studentClassIds = entryList.Select(e => e.StudentClassId).ToList();

        List<Assessment> existing = await Set
            .Where(a => a.ExamId == examId && a.ClassSubjectId == classSubjectId && studentClassIds.Contains(a.StudentClassId))
            .ToListAsync();

        Dictionary<int, Assessment> existingByScId = existing.ToDictionary(a => a.StudentClassId);

        foreach ((int studentClassId, decimal score) in entryList)
        {
            if (existingByScId.TryGetValue(studentClassId, out Assessment? assessment))
            {
                assessment.TotalScore = score;
            }
            else
            {
                Assessment newAssessment = new()
                {
                    TotalScore = score,
                    ExamId = examId,
                    StudentClassId = studentClassId,
                    ClassSubjectId = classSubjectId
                };
                await Set.AddAsync(newAssessment);
            }
        }

        await Context.SaveChangesAsync();
    }
}
