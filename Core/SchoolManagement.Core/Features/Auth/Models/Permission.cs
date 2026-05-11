using System.ComponentModel;

namespace SchoolManagement.Core.Models
{
    public class Permission
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("ឈ្មោះ")]
        public string Name { get; set; } = "";
        [Description("មុខងារ")]
        public ICollection<Role> Roles { get; set; } = [];
    }
}
