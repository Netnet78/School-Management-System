using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Features.Skills.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Presentation.Shared.Features.Candidates.Observables
{
    public partial class CandidateForm : ObservableValidator
    {
        [Description("លេខសម្គាល់")]
        [ObservableProperty]
        private int _id;

        // ====== IMPORTANT INFORMATION ======

        [Required(AllowEmptyStrings = false, ErrorMessage = "គោត្តនាម​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("គោត្តនាម")]
        [ObservableProperty]
        private string _firstName = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "គោត្តនាម​ឡាតាំង​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("គោត្តនាម (ឡាតាំង)")]
        [ObservableProperty]
        private string _latinFirstName = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "នាម​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("នាម")]
        [ObservableProperty]
        private string _lastName = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "នាមឡាតាំង​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("នាម (ឡាតាំង)")]
        [ObservableProperty]
        private string _latinLastName = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "ភេទ​មិន​អាច​គ្មាន​ទិន្នន័យ​បានទេ")]
        [Description("ភេទ")]
        [ObservableProperty]
        private Gender _gender;

        [Description("កាលបរិច្ឆេទកំណើត")]
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Age))] 
        private DateOnly? _dateOfBirth;

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
        [ObservableProperty]
        private int _skillId;

        [Required]
        [Description("ជំនាញ")]
        [ObservableProperty]
        private Skill? _skill;

        [Description("កាលបរិច្ឆេទបង្កើត")]
        [ObservableProperty]
        private DateTime _createdAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

        // ====== PERSONAL INFORMATION ======

        [Description("ភូមិកំណើត")]
        [ObservableProperty]
        private string _birthVillage = string.Empty;

        [Description("ឃុំកំណើត")]
        [ObservableProperty]
        private string _birthCommune = string.Empty;

        [Description("ស្រុកកំណើត")]
        [ObservableProperty]
        private string _birthDistrict = string.Empty;

        [Description("ខេត្តកំណើត")]
        [ObservableProperty]
        private string _birthProvince = string.Empty;

        [Description("ឈ្មោះឪពុក")]
        [ObservableProperty]
        private string _fatherName = string.Empty;

        [Description("ឈ្មោះម្ដាយ")]
        [ObservableProperty]
        private string _motherName = string.Empty;

        [Description("មុខរបរឪពុក")]
        [ObservableProperty]
        private string _fatherOccupation = string.Empty;

        [Description("មុខរបរម្ដាយ")]
        [ObservableProperty]
        private string _motherOccupation = string.Empty;

        [Description("ចំនួនបងប្អូន")]
        [ObservableProperty]
        private int _siblingsCount = 1;

        [Description("សាសនា")]
        [ObservableProperty]
        private string _religion = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PhotoKey))]
        private StudentPhoto? _photo;

        [Description("រូបភាព")]
        public string PhotoKey => Photo == null ? string.Empty : Photo.Key ?? string.Empty;

        [Description("លេខទូរស័ព្ទ")]
        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        // ====== EXAM & SCHOOL ======

        [Description("មណ្ឌលប្រឡង")]
        [ObservableProperty]
        private string _examCenter = string.Empty;

        [Description("កាលបរិច្ឆេទប្រឡង")]
        [ObservableProperty]
        private DateOnly? _examDate = DateOnly.FromDateTime(DateTime.Today);

        [Description("លេខតុ")]
        [ObservableProperty]
        private int? _examTable;

        [Description("លេខបន្ទប់")]
        [ObservableProperty]
        private int? _examRoom;

        [Description("ឈ្មោះសាលារៀន")]
        [ObservableProperty]
        private string _fromSchool = string.Empty;

        // ====== OTHER ======

        [Description("ការស្នាក់នៅ")]
        [ObservableProperty]
        private StudentStayType _stayType = StudentStayType.Outside;

        [Description("ព័ត៌មានផ្សេងៗ")]
        [ObservableProperty]
        private string _otherInfo = string.Empty;
    }
}
