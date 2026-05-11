using System.ComponentModel;

namespace SchoolManagement.Core.Models
{
    public class Department
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("ឈ្មោះ")]
        public string Name { get; set; } = "";
        [Description("ការពណ៌នា")]
        public string Description { get; set; } = string.Empty;
        [Description("កំពុងដំណើរការ")]
        public bool IsActive { get; set; } = true;
        public ICollection<Generation> Generations { get; set; } = [];
        public ICollection<Employee> Employees { get; set; } = [];
    }
}
