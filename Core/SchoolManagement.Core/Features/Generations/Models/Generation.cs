using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Generations.Models
{
    [Description("ជំនាន់")]
    public class Generation : IEntity
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
        public string KhmerName => $"ជំនាន់ទី {CohortNumber}";
    }
}
