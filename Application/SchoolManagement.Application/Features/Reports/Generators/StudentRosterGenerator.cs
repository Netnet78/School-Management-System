using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Shared.Models;
using SchoolManagement.Infrastructure.Features.Students.Contracts;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    public class StudentRosterGenerator : IReportGenerator
    {
        private readonly IStudentClassRepository _studentClassRepository;

        public string ReportTypeKey => "student-roster";

        public StudentRosterGenerator(IStudentClassRepository studentClassRepository)
        {
            _studentClassRepository = studentClassRepository;
        }

        public object CreateDefaultFilter() => new StudentRosterFilter
        {
            IsActive = true
        };

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var studentFilter = (StudentRosterFilter)filter;

            var filters = new List<FilterCondition<StudentClass>>();

            if (studentFilter.ClassId.HasValue)
                filters.Add(new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, studentFilter.ClassId.Value));

            if (studentFilter.GradeId.HasValue)
                filters.Add(new FilterCondition<StudentClass>(sc => sc.Class.GradeId, FilterOperator.Equals, studentFilter.GradeId.Value));

            if (studentFilter.IsActive.HasValue)
                filters.Add(new FilterCondition<StudentClass>(sc => sc.IsActive, FilterOperator.Equals, studentFilter.IsActive.Value));

            var studentClasses = await _studentClassRepository.FindAsync(
                filters,
                page: null,
                pageSize: null,
                orderBy: new[] { new SortCriteria<StudentClass>(sc => sc.Class.GradeId, OrderDirection.Ascending) },
                "Student.Candidate", "Class.Grade", "Class.Generation.Department");

            var rows = new List<Dictionary<string, object?>>();

            foreach (var sc in studentClasses)
            {
                var student = sc.Student;
                var candidate = student?.Candidate;
                var @class = sc.Class;

                if (studentFilter.SkillId.HasValue && candidate?.SkillId != studentFilter.SkillId.Value)
                    continue;

                rows.Add(new Dictionary<string, object?>
                {
                    ["fullName"] = candidate?.FullName,
                    ["latinFullName"] = candidate?.LatinFullName,
                    ["gender"] = candidate?.Gender.ToString(),
                    ["dateOfBirth"] = candidate?.DateOfBirth?.ToString("dd/MM/yyyy"),
                    ["className"] = @class?.KhmerName,
                    ["grade"] = @class?.Grade?.KhmerName,
                    ["skill"] = candidate?.Skill?.KhmerName,
                    ["status"] = student?.IsActiveReadable,
                    ["enrollDate"] = student?.EnrollDate?.ToString("dd/MM/yyyy"),
                });
            }

            return new ReportResult
            {
                Title = "បញ្ជីឈ្មោះសិស្សានុសិស្ស",
                SubTitle = "Student Roster Report",
                GeneratedDate = DateTime.UtcNow,
                Columns =
                [
                    new() { Key = "fullName", Header = "Full Name", HeaderKhmer = "ឈ្មោះពេញ", Width = 200 },
                    new() { Key = "latinFullName", Header = "Latin Name", HeaderKhmer = "ឈ្មោះឡាតាំង", Width = 180 },
                    new() { Key = "gender", Header = "Gender", HeaderKhmer = "ភេទ", Width = 80 },
                    new() { Key = "dateOfBirth", Header = "DOB", HeaderKhmer = "ថ្ងៃខែឆ្នាំកំណើត", Width = 120 },
                    new() { Key = "className", Header = "Class", HeaderKhmer = "ថ្នាក់", Width = 200 },
                    new() { Key = "grade", Header = "Grade", HeaderKhmer = "កម្រិត", Width = 100 },
                    new() { Key = "skill", Header = "Skill", HeaderKhmer = "ជំនាញ", Width = 120 },
                    new() { Key = "status", Header = "Status", HeaderKhmer = "ស្ថានភាព", Width = 100 },
                    new() { Key = "enrollDate", Header = "Enroll Date", HeaderKhmer = "ថ្ងៃចុះឈ្មោះ", Width = 120 },
                ],
                Rows = rows,
                Summary = new Dictionary<string, object>
                {
                    ["totalStudents"] = rows.Count,
                    ["totalFemale"] = rows.Count(r => r.GetValueOrDefault("gender")?.ToString() == "Female"),
                }
            };
        }
    }
}
