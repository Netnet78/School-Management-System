namespace School_Management.Core.Models
{
    public class StudentClass
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;
        public bool IsActive { get; set; } = false;

        public ICollection<Score> Scores { get; set; } = [];
        public ICollection<Attendance> Attendances { get; set; } = [];
    }
}
