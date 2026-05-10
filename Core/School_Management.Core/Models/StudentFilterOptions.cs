using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using System.ComponentModel;

namespace School_Management.Core.Models
{
    public class StudentFilterOptions
    {
        public string? Search { get; set; }
        public StudentField? SearchField { get; set; }
        public StudentDataStateFilterOptions DataState { get; set; } = StudentDataStateFilterOptions.All;
        public Skill? Skill { get; set; }
        public bool? IsActive { get; set; }
        public bool IncludeInActive { get; set; } = false;
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
