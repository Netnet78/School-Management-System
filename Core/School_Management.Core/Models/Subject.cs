using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<ClassSubject> ClassSubjects { get; set; } = [];
    }
}
