using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;

namespace SchoolManagement.Core.Models
{
    public class Student : IAuditableEntity
    {
        public int Id { get; set; }
        public DateOnly? EnrollDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // ====== IMPORTANT INFORMATION ======
        public string FirstName => Candidate.FirstName;

        public string LatinFirstName => Candidate.LatinFirstName;

        public string LastName => Candidate.LastName;

        public string LatinLastName => Candidate.LatinLastName;

        public string FullName => Candidate.FullName;

        public string LatinFullName => Candidate.LatinFullName;

        public Gender Gender => Candidate.Gender;

        public DateOnly? DateOfBirth => Candidate.DateOfBirth;

        public int? Age => Candidate.Age;

        public int SkillId => Candidate.SkillId;

        public Skill Skill => Candidate.Skill;

        public Department? Department => Classes.Count != 0 ?
            Classes.First().Class.Department : null;

        // ====== PERSONAL INFORMATION ======
        public string BirthVillage => Candidate.BirthVillage;

        public string BirthCommune => Candidate.BirthCommune;

        public string BirthDistrict => Candidate.BirthDistrict;

        public string BirthProvince => Candidate.BirthProvince;

        public string FatherName => Candidate.FatherName;

        public string MotherName => Candidate.MotherName;

        public string FatherOccupation => Candidate.FatherOccupation;

        public string MotherOccupation => Candidate.MotherOccupation;

        public int SiblingsCount => Candidate.SiblingsCount;

        public string Religion => Candidate.Religion;

        public StudentPhoto? Photo => Candidate.Photo;

        public string PhotoKey => Candidate.PhotoKey;

        public string PhoneNumber => Candidate.PhoneNumber;

        // ====== EXAM & SCHOOL ======

        public string ExamCenter => Candidate.ExamCenter;

        public DateOnly? ExamDate => Candidate.ExamDate;

        public int? ExamTable => Candidate.ExamTable;

        public int? ExamRoom => Candidate.ExamRoom;

        public string FromSchool => Candidate.FromSchool;

        // ====== OTHER ======

        public StudentStayType StayType => Candidate.StayType;

        public string OtherInfo => Candidate.OtherInfo;
        public bool IsActive { get; set; } = true;
        public string IsActiveReadable => IsActive ? "នៅរៀន" : "ឈប់រៀន";
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<StudentClass> Classes { get; set; } = [];
        public StudentQR? StudentQR { get; set; } = null;

        public string GetAuditName()
        {
            return $"{LastName} {FirstName}";
        }
    }
}
