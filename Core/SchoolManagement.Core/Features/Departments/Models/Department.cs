using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Generations.Models;
using SchoolManagement.Core.Features.Skills.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Departments.Models
{
    [Description("នាយកដ្ឋាន")]
    public class Department : IEntity
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("ឈ្មោះ")]
        public string Name { get; set; } = "";
        [Description("ឈ្មោះខ្មែរ")]
        public string KhmerName { get; set; } = "";
        [Description("ការពណ៌នា")]
        public string Description { get; set; } = string.Empty;
        [Description("កំពុងដំណើរការ")]
        public bool IsActive { get; set; } = true;
        public ICollection<Generation> Generations { get; set; } = [];
        public ICollection<Employee> Employees { get; set; } = [];
        public Skill Skill { get; set; } = null!;
    }
}
