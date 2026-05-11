using System.ComponentModel;

namespace SchoolManagement.Core.Enums
{
    public enum StudentStayType
    {
        [Description("ស្នាក់ក្នុង")]
        Inside,
        [Description("ស្នាក់ក្រៅ")]
        Outside,
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
}
