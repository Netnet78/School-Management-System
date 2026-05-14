using SchoolManagement.Core.Shared.Enums;
using SchoolManagement.Core.Features.Students.Enums;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Generations.Models;
using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Skills.Models;

namespace SchoolManagement.Core.Features.Students.Models
{
    public class StudentFilterOptions
    {
        public string? Search { get; set; }
        public StudentField? SearchField { get; set; }
        public StudentDataStateFilterOptions DataState { get; set; } = StudentDataStateFilterOptions.All;
        public Skill? Skill { get; set; }
        public bool? IsActive { get; set; }
        public bool IncludeInActive => IsActive == null;
        public Gender? Gender { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }
        public StudentStayType? StayType { get; set; }
        public OrderType OrderBy { get; set; }
        public Department? Department { get; set; }
        public Class? Class { get; set; }
        public Generation? Generation { get; set; }
        public Employee? Employee => Class?.Teacher;

        public StudentFilterOptions()
        {

        }
    }
}
