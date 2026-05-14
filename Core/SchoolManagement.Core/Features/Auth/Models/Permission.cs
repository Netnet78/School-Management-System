using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Auth.Models
{
    public class Permission : IEntity
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("ឈ្មោះ")]
        public string Name { get; set; } = "";
        [Description("មុខងារ")]
        public ICollection<Role> Roles { get; set; } = [];
    }
}
