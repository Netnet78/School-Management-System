using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Skills.Models
{
    public class Skill : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<Candidate> Students = [];
    }
}
