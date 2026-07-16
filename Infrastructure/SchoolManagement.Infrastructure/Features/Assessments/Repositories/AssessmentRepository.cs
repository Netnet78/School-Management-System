using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Assessments.Repositories;

public class AssessmentRepository : BaseRepository<Assessment>, IAssessmentRepository
{
    public AssessmentRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task UpsertRangeAsync(int examId, int classSubjectId, IEnumerable<(int StudentClassId, int MapperId, int ComponentId, decimal ScoreAmount)> entries)
    {
        List<(int StudentClassId, int MapperId, int ComponentId, decimal ScoreAmount)> entryList = entries.ToList();
        if (entryList.Count == 0) return;

        var grouped = entryList.GroupBy(e => e.StudentClassId);
        List<int> studentIds = grouped.Select(g => g.Key).ToList();

        Dictionary<int, Assessment> assessments = await Set
            .Include(a => a.Scores)
            .Where(a =>
                a.ExamId == examId &&
                a.ClassSubjectId == classSubjectId &&
                studentIds.Contains(a.StudentClassId))
            .ToDictionaryAsync(a => a.StudentClassId);

        foreach (var group in grouped)
        {
            if (assessments.TryGetValue(group.Key, out Assessment? existing))
            {
                foreach (var (_, mapperId, componentId, scoreAmount) in group)
                {
                    Score? score = existing.Scores.FirstOrDefault(s => s.MapperId == mapperId);
                    if (score != null)
                    {
                        score.Amount = scoreAmount;
                    }
                    else
                    {
                        Score newScore = new()
                        {
                            AssessmentId = existing.Id,
                            MapperId = mapperId,
                            ComponentId = componentId,
                            Amount = scoreAmount
                        };

                        existing.Scores.Add(newScore);
                    }
                }

                existing.TotalScore = existing.Scores.Sum(s => s.Amount);
            }
            else
            {
                Assessment assessment = new()
                {
                    ExamId = examId,
                    StudentClassId = group.Key,
                    ClassSubjectId = classSubjectId,
                    Scores = group.Select(e => new Score
                    {
                        MapperId = e.MapperId,
                        ComponentId = e.ComponentId,
                        Amount = e.ScoreAmount,
                    }).ToList()
                };
                assessment.TotalScore = assessment.Scores.Sum(s => s.Amount);
                await Set.AddAsync(assessment);
            }
        }

        await Context.SaveChangesAsync();
    }
    
}