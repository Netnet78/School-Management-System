namespace School_Management.Core.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Class> Classes { get; set; } = [];
    }
}
