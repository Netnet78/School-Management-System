using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Presentation.Shared.Features.Candidates.Observables;

namespace SchoolManagement.Presentation.Shared.Features.Students.Observables
{
    public partial class StudentForm : CandidateForm
    {
        public new int Id { get; set; }
        public int CandidateId { get; set; }
        public DateOnly? EnrollDate { get; set; }
        public StudentClass? Class { get; set; }
        public bool IsActive { get; set; } = true;
        public StudentQR? StudentQR { get; set; }
        public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public StudentForm(Student student)
        {
            Id = student.Id;
            IsActive = student.IsActive;
            CandidateId = student.CandidateId;
            CreatedAt = student.CreatedAt ?? CreatedAt;
            SkillId = student.SkillId;
            FirstName = student.FirstName;
            LastName = student.LastName;
            LatinFirstName = student.LatinFirstName;
            LatinLastName = student.LatinLastName;
            Gender = student.Gender;
            DateOfBirth = student.DateOfBirth;
            Skill = student.Skill;
            BirthVillage = student.BirthVillage;
            BirthCommune = student.BirthCommune;
            BirthDistrict = student.BirthDistrict;
            BirthProvince = student.BirthProvince;
            FatherName = student.FatherName;
            MotherName = student.MotherName;
            FatherOccupation = student.FatherOccupation;
            MotherOccupation = student.MotherOccupation;
            SiblingsCount = student.SiblingsCount;
            Religion = student.Religion;
            PhoneNumber = student.PhoneNumber;
            FromSchool = student.FromSchool;
            ExamCenter = student.ExamCenter;
            ExamDate = student.ExamDate;
            ExamTable = student.ExamTable;
            ExamRoom = student.ExamRoom;
            StayType = student.StayType;
            OtherInfo = student.OtherInfo;
            Photo = student.Photo;
        }

        public Student ToStudentModel()
        {
            Candidate candidate = new()
            {
                Id = CandidateId,
                FirstName = FirstName,
                LastName = LastName,
                LatinFirstName = LatinFirstName,
                LatinLastName = LatinLastName,
                Gender = Gender,
                DateOfBirth = DateOfBirth,
                SkillId = SkillId,
                Skill = Skill ?? new() { Id = SkillId },
                BirthVillage = BirthVillage,
                BirthCommune = BirthCommune,
                BirthDistrict = BirthDistrict,
                BirthProvince = BirthProvince,
                FatherName = FatherName,
                MotherName = MotherName,
                FatherOccupation = FatherOccupation,
                MotherOccupation = MotherOccupation,
                SiblingsCount = SiblingsCount,
                Religion = Religion,
                PhoneNumber = PhoneNumber,
                FromSchool = FromSchool,
                ExamCenter = ExamCenter,
                ExamDate = ExamDate,
                ExamTable = ExamTable,
                ExamRoom = ExamRoom,
                StayType = StayType,
                OtherInfo = OtherInfo,
            };

            return new()
            {
                Id = Id,
                CandidateId = CandidateId,
                Candidate = candidate,
                IsActive = IsActive,
                StudentQR = StudentQR,
                EnrollDate = EnrollDate,
                CreatedAt = CreatedAt,
            };
        }

        public StudentForm()
        {

        }

    }
}
