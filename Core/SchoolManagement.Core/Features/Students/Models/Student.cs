
using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Notifications.Models;
using SchoolManagement.Core.Features.Skills.Models;
using SchoolManagement.Core.Features.Students.Enums;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Shared.Enums;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Students.Models
{
    [Description("សិស្ស")]
    public class Student : IAuditableEntity, IEntity
    {
        public int Id { get; set; }
        public DateOnly? EnrollDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        // ====== IMPORTANT INFORMATION ======
        public string FirstName => Candidate?.FirstName ?? string.Empty;

        public string LatinFirstName => Candidate?.LatinFirstName ?? string.Empty;

        public string LastName => Candidate?.LastName ?? string.Empty;

        public string LatinLastName => Candidate?.LatinLastName ?? string.Empty;

        public string FullName => Candidate?.FullName ?? string.Empty;

        public string LatinFullName => Candidate?.LatinFullName ?? string.Empty;

        public Gender Gender => Candidate?.Gender ?? default;

        public DateOnly? DateOfBirth => Candidate?.DateOfBirth;

        public int? Age => Candidate?.Age;

        public int SkillId => Candidate?.SkillId ?? default;

        public Skill Skill => Candidate?.Skill ?? null!;

        public Department? Department => Classes.FirstOrDefault()?.Class?.Department;

        // ====== PERSONAL INFORMATION ======
        public string BirthVillage => Candidate?.BirthVillage ?? string.Empty;

        public string BirthCommune => Candidate?.BirthCommune ?? string.Empty;

        public string BirthDistrict => Candidate?.BirthDistrict ?? string.Empty;

        public string BirthProvince => Candidate?.BirthProvince ?? string.Empty;

        public string FatherName => Candidate?.FatherName ?? string.Empty;

        public string MotherName => Candidate?.MotherName ?? string.Empty;

        public string FatherOccupation => Candidate?.FatherOccupation ?? string.Empty;

        public string MotherOccupation => Candidate?.MotherOccupation ?? string.Empty;

        public int SiblingsCount => Candidate?.SiblingsCount ?? default;

        public string Religion => Candidate?.Religion ?? string.Empty;

        public StudentPhoto? Photo => Candidate?.Photo;

        public string PhotoKey => Candidate?.PhotoKey ?? string.Empty;

        public string PhoneNumber => Candidate?.PhoneNumber ?? string.Empty;

        // ====== EXAM & SCHOOL ======

        public string ExamCenter => Candidate?.ExamCenter ?? string.Empty;

        public DateOnly? ExamDate => Candidate?.ExamDate;

        public int? ExamTable => Candidate?.ExamTable;

        public int? ExamRoom => Candidate?.ExamRoom;

        public string FromSchool => Candidate?.FromSchool ?? string.Empty;

        // ====== OTHER ======

        public StudentStayType StayType => Candidate?.StayType ?? default;

        public string OtherInfo => Candidate?.OtherInfo ?? string.Empty;
        public bool IsActive { get; set; } = true;
        public string IsActiveReadable => IsActive ? "នៅរៀន" : "ឈប់រៀន";
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<StudentClass> Classes { get; set; } = [];
        public StudentQR? StudentQR { get; set; } = null;

        public string CustomAuditDescription()
        {
            return "";
        }

        public string CustomAuditName()
        {
            return string.IsNullOrWhiteSpace(FullName)
                ? $"Student #{Id}"
                : FullName;
        }
    }
}
