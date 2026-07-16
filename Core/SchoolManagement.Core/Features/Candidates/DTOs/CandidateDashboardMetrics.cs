using SchoolManagement.Core.Features.Candidates.Models;

namespace SchoolManagement.Core.Features.Candidates.DTOs
{
    public class CandidateDashboardMetrics
    {
        public Dictionary<string, int> GenderDistribution { get; set; } = new();
        public Dictionary<string, int> SkillDistribution { get; set; } = new();
        
        // Maps the Date (e.g. 2026-07-10) to the number of candidates joined
        public Dictionary<DateTime, int> JoinedRate { get; set; } = new();
        
        public List<Candidate> InsertedToday { get; set; } = new();
    }
}
