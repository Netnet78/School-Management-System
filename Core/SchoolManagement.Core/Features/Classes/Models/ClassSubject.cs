namespace SchoolManagement.Core.Models
{
    public class ClassSubject
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;
        public int TeacherId { get; set; }
        public Employee Employee { get; set; } = null!;
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public ICollection<Score> Scores { get; set; } = [];
    }
}
