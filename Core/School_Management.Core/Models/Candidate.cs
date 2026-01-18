using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    public class Candidate
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
