using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Student_Management.Models
{
    public enum StudentStayType
    {
        [Description("ស្នាក់ក្នុង")]
        Inside,
        [Description("ស្នាក់ក្រៅ")]
        Outside,
    }
    public enum StudentGender
    {
        [Description("ប្រុស")]
        Male,
        [Description("ស្រី")]
        Female,
    }

    public enum StudentSkill
    {
        [Description("កុំព្យូទ័រ")]
        Computer,
        [Description("អគ្គិសនី")]
        Electrical,
        [Description("មេកានិច")]
        CNC,
        [Description("ចំណេះទូទៅ")]
        General,
    }
    public class Student
    {
        public int Id { get; set; }

        // Important information
        [Required]
        required public string FirstName { get; set; }
        [Required]
        required public string LastName { get; set; }
        public string FullName => $"{LastName} {FirstName}";
        [Required]
        required public StudentGender Gender { get; set; }
        [Required]
        required public DateOnly DateOfBirth { get; set; }
        public int Age
        {
            get
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                int age = today.Year - DateOfBirth.Year;
                if (DateOfBirth > today.AddYears(-age)) age--;
                return age;
            }
        }
        [Required]
        required public StudentSkill Skill { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

        // Other important personal information
        public string BirthVillage { get; set; } = "";
        public string BirthCommune { get; set; } = "";
        public string BirthDistrict { get; set; } = "";
        public string BirthProvince { get; set; } = "";
        public string FatherName { get; set; } = "";
        public string MotherName { get; set; } = "";
        public string FatherOccupation { get; set; } = "";
        public string MotherOccupation { get; set; } = "";
        public int SiblingsCount { get; set; } = 0;
        public string Religion { get; set; } = "";
        public string PhotoPath { get; set; } = "";

        // Exam & School Information
        public string ExamCenter { get; set; } = "";
        public string ExamDate { get; set; } = "";
        public string ExamTable { get; set; } = "";
        public string ExamRoom { get; set; } = "";
        public string FromSchool { get; set; } = "";

        // Other Information (For students come from other provinces)
        public StudentStayType StayType { get; set; } = StudentStayType.Outside;
        public string OtherInfo { get; set; } = "";
    }
}
