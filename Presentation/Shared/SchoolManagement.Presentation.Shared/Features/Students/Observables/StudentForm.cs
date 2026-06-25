using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Presentation.Shared.Features.Candidates.Observables;

namespace SchoolManagement.Presentation.Shared.Features.Students.Observables
{
    public partial class StudentForm : CandidateForm
    {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private int _candidateId;
        [ObservableProperty]
        private DateTime? _enrollDate;
        [ObservableProperty]
        private StudentClass? _class;
        [ObservableProperty]
        private bool _isActive;
        [ObservableProperty]
        private StudentQR? _studentQr;
        [ObservableProperty]
        private DateTime _candidateCreatedAt;

        public StudentForm(Student student)
        {
            Id = student.Id;
            IsActive = student.IsActive;
            CandidateId = student.CandidateId;
            CreatedAt = student.CreatedAt ?? CreatedAt;
            CandidateCreatedAt = student.Candidate.CreatedAt;
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
            EnrollDate = student.EnrollDate?.ToDateTime(TimeOnly.MinValue);
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
                Skill = Skill!,
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
                CreatedAt = CandidateCreatedAt
            };

            return new Student()
            {
                Id = Id,
                CandidateId = CandidateId,
                Candidate = candidate,
                IsActive = IsActive,
                StudentQR = StudentQr,
                EnrollDate = EnrollDate is DateTime dt ? DateOnly.FromDateTime(dt) : DateOnly.FromDateTime(DateTime.Now),
                CreatedAt = CreatedAt,
            };
        }

        public StudentForm()
        {

        }

    }
}
