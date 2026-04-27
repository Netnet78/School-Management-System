using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    public class StudentFilterOptions
    {
        public string? Search { get; set; }
        public StudentField? SearchField { get; set; }
        public StudentDataStateFilterOptions DataState { get; set; }
        public bool? IsActive { get; set; }
        public string? Gender { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }
        public OrderType OrderBy { get; set; }
        //public bool SortType { get; set; } = true;

    }
}
