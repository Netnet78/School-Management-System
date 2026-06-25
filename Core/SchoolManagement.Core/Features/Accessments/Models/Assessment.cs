using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Exams.Models;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Assessments.Models
{
    [Description("លទ្ធផលសមត្ថភាព")]
    public class Assessment : IEntity
    {
        public int Id { get; set; }
        public decimal TotalScore { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;
        public int StudentClassId { get; set; }
        public StudentClass StudentClass { get; set; } = null!;
        public int ClassSubjectId { get; set; }
        public ClassSubject ClassSubject { get; set; } = null!;
        public string OtherInfo { get; set; } = string.Empty;
        public ICollection<Score> Scores { get; set; } = [];
    }
}
