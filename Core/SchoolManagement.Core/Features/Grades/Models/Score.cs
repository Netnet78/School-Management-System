using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Exams.Models;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Grades.Models
{
    public class Score : IEntity
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;
        public int StudentClassId { get; set; }
        public StudentClass StudentClass { get; set; } = null!;
        public int ClassSubjectId { get; set; }
        public ClassSubject ClassSubject { get; set; } = null!;
    }
}
