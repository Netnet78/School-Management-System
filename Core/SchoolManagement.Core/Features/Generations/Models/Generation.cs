using System.ComponentModel;

namespace SchoolManagement.Core.Models
{
    public class Generation
    {
        [Description("លេខសម្គាល់")]
        public int Id { get; set; }
        [Description("លេខជំនាន់")]
        public int CohortNumber { get; set; }
        [Description("ឆ្នាំចាប់ផ្ដើម")]
        public int AcademicStartYear { get; set; }
        [Description("ឆ្នាំបញ្ចប់")]
        public int AcademicEndYear { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        public ICollection<Class> Classes { get; set; } = [];
    }
}
