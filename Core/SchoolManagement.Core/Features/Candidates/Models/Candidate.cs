
using SchoolManagement.Core.Features.Skills.Models;
using SchoolManagement.Core.Features.Students.Enums;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Shared.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Core.Features.Candidates.Models
{
    [Description("បេក្ខជន")]
    public class Candidate : IEntity, IAuditableEntity
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }

        // ====== IMPORTANT INFORMATION ======

        [Required(AllowEmptyStrings = false, ErrorMessage = "គោត្តនាម​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("គោត្តនាម")]
        public string FirstName { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "គោត្តនាម​ឡាតាំង​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("គោត្តនាម (ឡាតាំង)")]
        public string LatinFirstName { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "នាម​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("នាម")]
        public string LastName { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "នាមឡាតាំង​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("នាម (ឡាតាំង)")]
        public string LatinLastName { get; set; } = null!;

        [Description("ឈ្មោះពេញ")]
        public string FullName { get; private set; } = null!;

        [Description("ឈ្មោះពេញ (ឡាតាំង)")]
        public string LatinFullName { get; private set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "ភេទ​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("ភេទ")]
        public Gender Gender { get; set; }

        [Description("កាលបរិច្ឆេទកំណើត")]
        public DateOnly? DateOfBirth { get; set; }

        [Description("អាយុ")]
        public int? Age
        {
            get
            {
                if (DateOfBirth is null)
                    return null;

                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                DateOnly dob = DateOfBirth.Value;

                int age = today.Year - dob.Year;

                if (today < dob.AddYears(age))
                    age--;

                return age;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "លេខសម្គាល់ជំនាញមិនអាចគ្មានទិន្នន័យបានទេ")]
        [Description("លេខសម្គាល់ជំនាញ")]
        public int SkillId { get; set; }

        [Required]
        [Description("ជំនាញ")]
        public Skill Skill { get; set; } = null!;

        [Description("កាលបរិច្ឆេទបង្កើត")]
        public DateTime CreatedAt { get; set; }
            = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

        // ====== PERSONAL INFORMATION ======

        [Description("ភូមិកំណើត")]
        public string BirthVillage { get; set; } = string.Empty;

        [Description("ឃុំកំណើត")]
        public string BirthCommune { get; set; } = string.Empty;

        [Description("ស្រុកកំណើត")]
        public string BirthDistrict { get; set; } = string.Empty;

        [Description("ខេត្តកំណើត")]
        public string BirthProvince { get; set; } = string.Empty;

        [Description("ឈ្មោះឪពុក")]
        public string FatherName { get; set; } = string.Empty;

        [Description("ឈ្មោះម្ដាយ")]
        public string MotherName { get; set; } = string.Empty;

        [Description("មុខរបរឪពុក")]
        public string FatherOccupation { get; set; } = string.Empty;

        [Description("មុខរបរម្ដាយ")]
        public string MotherOccupation { get; set; } = string.Empty;

        [Description("ចំនួនបងប្អូន")]
        public int SiblingsCount { get; set; } = 1;

        [Description("សាសនា")]
        public string Religion { get; set; } = string.Empty;
        public StudentPhoto? Photo { get; set; }

        [Description("រូបភាព")]
        public string PhotoKey => Photo == null ? string.Empty : Photo.Key ?? string.Empty;

        [Description("លេខទូរស័ព្ទ")]
        public string PhoneNumber { get; set; } = string.Empty;

        // ====== EXAM & SCHOOL ======

        [Description("មណ្ឌលប្រឡង")]
        public string ExamCenter { get; set; } = string.Empty;

        [Description("កាលបរិច្ឆេទប្រឡង")]
        public DateOnly? ExamDate { get; set; }
            = DateOnly.FromDateTime(DateTime.Today);

        [Description("លេខតុ")]
        public int? ExamTable { get; set; }

        [Description("លេខបន្ទប់")]
        public int? ExamRoom { get; set; }

        [Description("ឈ្មោះសាលារៀន")]
        public string FromSchool { get; set; } = string.Empty;

        // ====== OTHER ======

        [Description("ការស្នាក់នៅ")]
        public StudentStayType StayType { get; set; } = StudentStayType.Outside;

        [Description("ព័ត៌មានផ្សេងៗ")]
        public string OtherInfo { get; set; } = string.Empty;

        // One candidate can be a student (one to unique)
        public Student? Student { get; set; } = null;

        public string CustomAuditDescription()
        {
            return "";
        }

        public string CustomAuditName()
        {
            return FullName;
        }
    }
}
