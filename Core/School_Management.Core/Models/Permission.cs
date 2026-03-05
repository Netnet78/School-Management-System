using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace School_Management.Core.Models
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
