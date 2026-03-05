using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Score> Scores { get; set; } = [];
    }
}
