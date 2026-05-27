namespace SchoolManagement.Application.Features.Reports.Models
{
    public class StudentRosterFilter
    {
        public int? GradeId { get; set; }

        public int? ClassId { get; set; }

        public int? SkillId { get; set; }

        public bool? IsActive { get; set; } = true;
    }
}
