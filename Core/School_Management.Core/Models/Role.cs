using System.ComponentModel;

namespace School_Management.Core.Models
{
    public class Role
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("ឈ្មោះ")]
        public string Name { get; set; } = "";
        [Description("ព័ត៌មានបន្ថែម")]
        public string? Description { get; set; }
        [Description("ការអនុញ្ញាត")]
        public ICollection<Permission> Permissions { get; set; } = [];
        public ICollection<User> Users { get; set; } = [];
    }
}
