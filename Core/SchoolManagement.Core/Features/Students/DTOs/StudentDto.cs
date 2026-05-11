namespace SchoolManagement.Core.Application.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LatinFullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }
}
