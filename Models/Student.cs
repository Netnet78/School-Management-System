using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace New_Student_Management.Models
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

    public enum StudentField
    {
        [Description("លេខសម្គាល់")]
        Id,

        // ====== IMPORTANT INFORMATION ======

        [Description("គោត្តនាម")]
        FirstName,

        [Description("គោត្តនាម (ឡាតាំង)")]
        LatinFirstName,

        [Description("នាម")]
        LastName,

        [Description("នាម (ឡាតាំង)")]
        LatinLastName,

        [Description("ឈ្មោះពេញ")]
        FullName,

        [Description("ឈ្មោះពេញ (ឡាតាំង)")]
        LatinFullName,

        [Description("ភេទ")]
        Gender,

        [Description("កាលបរិច្ឆេទកំណើត")]
        DateOfBirth,

        [Description("អាយុ")]
        Age,

        [Description("ជំនាញ")]
        Skill,

        [Description("កាលបរិច្ឆេទបង្កើត")]
        CreatedAt,

        // ====== PERSONAL INFORMATION ======

        [Description("ភូមិកំណើត")]
        BirthVillage,

        [Description("ឃុំកំណើត")]
        BirthCommune,

        [Description("ស្រុកកំណើត")]
        BirthDistrict,

        [Description("ខេត្តកំណើត")]
        BirthProvince,

        [Description("ឈ្មោះឪពុក")]
        FatherName,

        [Description("ឈ្មោះម្ដាយ")]
        MotherName,

        [Description("មុខរបរឪពុក")]
        FatherOccupation,

        [Description("មុខរបរម្ដាយ")]
        MotherOccupation,

        [Description("ចំនួនបងប្អូន")]
        SiblingsCount,

        [Description("សាសនា")]
        Religion,

        [Description("រូបភាព")]
        PhotoPath,

        [Description("លេខទូរស័ព្ទ")]
        PhoneNumber,

        // ====== EXAM & SCHOOL ======

        [Description("មណ្ឌលប្រឡង")]
        ExamCenter,

        [Description("កាលបរិច្ឆេទប្រឡង")]
        ExamDate,

        [Description("លេខតុ")]
        ExamTable,

        [Description("លេខបន្ទប់")]
        ExamRoom,

        [Description("ឈ្មោះសាលារៀន")]
        FromSchool,

        // ====== OTHER ======

        [Description("ការស្នាក់នៅ")]
        StayType,

        [Description("ព័ត៌មានផ្សេងៗ")]
        OtherInfo
    }

    public class Student
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }

        // ====== IMPORTANT INFORMATION ======

        [Required]
        [Description("គោត្តនាម")]
        public required string FirstName { get; set; }

        [Required]
        [Description("គោត្តនាម (ឡាតាំង)")]
        public required string LatinFirstName { get; set; }

        [Required]
        [Description("នាម")]
        public required string LastName { get; set; }

        [Required]
        [Description("នាម (ឡាតាំង)")]
        public required string LatinLastName { get; set; }

        [Description("ឈ្មោះពេញ")]
        public string FullName => $"{LastName} {FirstName}";

        [Description("ឈ្មោះពេញ (ឡាតាំង)")]
        public string LatinFullName => $"{LatinLastName} {LatinFirstName}";

        [Required]
        [Description("ភេទ")]
        public required StudentGender Gender { get; set; }

        [Required]
        [Description("កាលបរិច្ឆេទកំណើត")]
        public required DateOnly DateOfBirth { get; set; }

        [Description("អាយុ")]
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
        [Description("ជំនាញ")]
        public required StudentSkill Skill { get; set; }

        [Description("កាលបរិច្ឆេទបង្កើត")]
        public DateTime CreatedAt { get; set; }
            = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

        // ====== PERSONAL INFORMATION ======

        [Description("ភូមិកំណើត")]
        public string BirthVillage { get; set; } = "";

        [Description("ឃុំកំណើត")]
        public string BirthCommune { get; set; } = "";

        [Description("ស្រុកកំណើត")]
        public string BirthDistrict { get; set; } = "";

        [Description("ខេត្តកំណើត")]
        public string BirthProvince { get; set; } = "";

        [Description("ឈ្មោះឪពុក")]
        public string FatherName { get; set; } = "";

        [Description("ឈ្មោះម្ដាយ")]
        public string MotherName { get; set; } = "";

        [Description("មុខរបរឪពុក")]
        public string FatherOccupation { get; set; } = "";

        [Description("មុខរបរម្ដាយ")]
        public string MotherOccupation { get; set; } = "";

        [Description("ចំនួនបងប្អូន")]
        public int SiblingsCount { get; set; } = 0;

        [Description("សាសនា")]
        public string Religion { get; set; } = "";

        [Description("រូបភាព")]
        public string PhotoPath { get; set; } = "";

        [Description("លេខទូរស័ព្ទ")]
        public string PhoneNumber { get; set; } = "";

        // ====== EXAM & SCHOOL ======

        [Description("មណ្ឌលប្រឡង")]
        public string ExamCenter { get; set; } = "";

        [Description("កាលបរិច្ឆេទប្រឡង")]
        public DateOnly ExamDate { get; set; }
            = DateOnly.FromDateTime(DateTime.Today);

        [Description("លេខតុ")]
        public int? ExamTable { get; set; }

        [Description("លេខបន្ទប់")]
        public int? ExamRoom { get; set; }

        [Description("ឈ្មោះសាលារៀន")]
        public string FromSchool { get; set; } = "";

        // ====== OTHER ======

        [Description("ការស្នាក់នៅ")]
        public StudentStayType StayType { get; set; } = StudentStayType.Outside;

        [Description("ព័ត៌មានផ្សេងៗ")]
        public string OtherInfo { get; set; } = "";
    }
}
