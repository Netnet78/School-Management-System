using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Auth.Models
{
    [Description("តួនាទី")]
    public class Role : IEntity
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("ឈ្មោះ")]
        public string Name { get; set; } = "";
        [Description("ព័ត៌មានបន្ថែម")]
        public string? Description { get; set; }
        [Description("ការអនុញ្ញាត")]
        public List<Permission> Permissions { get; set; } = [];
        public List<User> Users { get; set; } = [];
    }
}
