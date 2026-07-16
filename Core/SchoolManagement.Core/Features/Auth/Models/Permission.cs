using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Auth.Models
{
    [Description("ការអនុញ្ញាត")]
    public class Permission : IEntity
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("ឈ្មោះ")]
        public string Name { get; set; } = "";
        [Description("មុខងារ")]
        public List<Role> Roles { get; set; } = [];
    }
}
