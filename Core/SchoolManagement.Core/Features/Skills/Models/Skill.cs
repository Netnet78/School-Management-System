using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Skills.Models
{
    [Description("ជំនាញ")]
    public class Skill : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Department Department { get; set; } = null!;
        public List<Candidate> Candidates { get; set; } = [];
    }
}
