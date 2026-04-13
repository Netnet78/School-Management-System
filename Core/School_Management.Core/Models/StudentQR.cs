namespace School_Management.Core.Models
{
    public class StudentQR
    {
        public int Id { get; set; }
        public string QRCodeValue { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
