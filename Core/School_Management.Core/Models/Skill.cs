using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;

        public ICollection<Candidate> Students = [];
    }
}
