using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class Class
    {
        public int Id { get; set; }
        public int GradeId { get; set; }
        public Grade Grade { get; set; } = null!;
        public int GenerationId { get; set; }
        public Generation Generation { get; set; } = null!;
        public int? TeacherId { get; set; }
        public Employee? Teacher { get; set; } = null;

        public ICollection<StudentClass> StudentClasses { get; set; } = [];
        public ICollection<ClassSubject> ClassSubjects { get; set; } = [];
    }
}
