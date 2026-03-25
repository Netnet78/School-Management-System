using School_Management.Core.Enums;
using School_Management.Core.Models;

namespace School_Management.Presentation.Shared.Helpers
{
    public static class StudentFilters
    {
        public static bool MatchSearch(
            Candidate student,
            string keyword,
            StudentField currentSearchField)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return true;

            keyword = keyword.Trim();

            return currentSearchField switch
            {
                // ===== IMPORTANT INFORMATION =====

                StudentField.Id =>
                    student.Id.ToString().Contains(keyword),

                StudentField.FullName =>
                    student.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.LatinFullName =>
                    student.LatinFullName.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.Gender =>
                    EnumExtensions.GetDescription(student.Gender)
                        .Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.DateOfBirth =>
                    student.DateOfBirth.ToString()
                        .Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.Age =>
                    student.Age.ToString()
                        .Contains(keyword),

                // ===== PERSONAL INFORMATION =====

                StudentField.BirthVillage =>
                    student.BirthVillage.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.BirthCommune =>
                    student.BirthCommune.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.BirthDistrict =>
                    student.BirthDistrict.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.BirthProvince =>
                    student.BirthProvince.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.FatherName =>
                    student.FatherName.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.MotherName =>
                    student.MotherName.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.FatherOccupation =>
                    student.FatherOccupation.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.MotherOccupation =>
                    student.MotherOccupation.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.SiblingsCount =>
                    student.SiblingsCount.ToString()
                        .Contains(keyword),

                StudentField.Religion =>
                    student.Religion.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.PhoneNumber =>
                    student.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                // ===== EXAM & SCHOOL =====

                StudentField.ExamCenter =>
                    student.ExamCenter.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.ExamDate =>
                    student.ExamDate.ToString()
                        .Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.ExamTable =>
                    student.ExamTable?.ToString()
                        .Contains(keyword) == true,

                StudentField.ExamRoom =>
                    student.ExamRoom?.ToString()
                        .Contains(keyword) == true,

                StudentField.FromSchool =>
                    student.FromSchool.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                // ===== OTHER =====

                StudentField.StayType =>
                    EnumExtensions.GetDescription(student.StayType)
                        .Contains(keyword, StringComparison.OrdinalIgnoreCase),

                StudentField.OtherInfo =>
                    student.OtherInfo.Contains(keyword, StringComparison.OrdinalIgnoreCase),

                // ===== IGNORED FIELDS =====
                // FirstName
                // LastName
                // LatinFirstName
                // LatinLastName
                // PhotoPath
                // CreatedAt

                _ => false
            };
        }
    }
}
