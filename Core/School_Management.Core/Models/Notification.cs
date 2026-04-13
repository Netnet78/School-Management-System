namespace School_Management.Core.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
    }
}
