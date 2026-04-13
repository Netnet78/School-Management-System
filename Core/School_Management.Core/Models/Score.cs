namespace School_Management.Core.Models
{
    public class Score
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
