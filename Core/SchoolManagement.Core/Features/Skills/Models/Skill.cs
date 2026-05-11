namespace SchoolManagement.Core.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<Candidate> Students = [];
    }
}
