using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    public class ScoreReportGenerator : IReportGenerator
    {
        private readonly IAccessmentRepository _assessmentRepository;
        private readonly IExamRepository _examRepository;

        public string ReportTypeKey => "score-report";

        public ScoreReportGenerator(
            IAccessmentRepository assessmentRepository,
            IExamRepository examRepository)
        {
            _assessmentRepository = assessmentRepository;
            _examRepository = examRepository;
        }

        public object CreateDefaultFilter() => new ScoreReportFilter();

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var scoreFilter = (ScoreReportFilter)filter;

            var filters = new List<FilterCondition<Assessment>>();

            if (scoreFilter.ExamId.HasValue)
                filters.Add(new FilterCondition<Assessment>(a => a.ExamId, FilterOperator.Equals, scoreFilter.ExamId.Value));

            var assessments = await _assessmentRepository.FindAsync(
                filters,
                page: null,
                pageSize: null,
                orderBy: new[] { new SortCriteria<Assessment>(a => a.StudentClass.Student.Candidate.FullName, OrderDirection.Ascending) },
                "StudentClass.Student.Candidate",
                "ClassSubject.Subject",
                "ClassSubject.Class",
                "Exam",
                "Scores.Component");

            // Filter by class/subject after retrieval
            var filtered = assessments.AsEnumerable();

            if (scoreFilter.ClassId.HasValue)
                filtered = filtered.Where(a => a.ClassSubject?.ClassId == scoreFilter.ClassId.Value);

            if (scoreFilter.SubjectId.HasValue)
                filtered = filtered.Where(a => a.ClassSubject?.SubjectId == scoreFilter.SubjectId.Value);

            var assessmentList = filtered.ToList();

            // Collect all unique component names across all assessments
            var allComponentKeys = assessmentList
                .SelectMany(a => a.Scores)
                .Where(s => s.Component != null)
                .Select(s => s.Component.Name)
                .Distinct()
                .Order()
                .ToList();

            var rows = new List<Dictionary<string, object?>>();

            foreach (var assessment in assessmentList)
            {
                var candidate = assessment.StudentClass?.Student?.Candidate;
                var subject = assessment.ClassSubject?.Subject;
                var exam = assessment.Exam;

                var row = new Dictionary<string, object?>
                {
                    ["studentName"] = candidate?.FullName,
                    ["latinName"] = candidate?.LatinFullName,
                    ["subject"] = subject?.KhmerName ?? subject?.Name,
                    ["exam"] = exam?.KhmerName ?? exam?.Name,
                    ["className"] = assessment.ClassSubject?.Class?.KhmerName,
                    ["totalScore"] = assessment.TotalScore,
                };

                // Add component scores as dynamic columns
                foreach (var componentKey in allComponentKeys)
                {
                    var score = assessment.Scores
                        .FirstOrDefault(s => s.Component?.Name == componentKey);

                    row[$"component_{componentKey}"] = score?.Amount;
                }

                rows.Add(row);
            }

            var columns = new List<ReportColumn>
            {
                new() { Key = "studentName", Header = "Student Name", HeaderKhmer = "ឈ្មោះសិស្ស", Width = 200 },
                new() { Key = "latinName", Header = "Latin Name", HeaderKhmer = "ឈ្មោះឡាតាំង", Width = 180 },
                new() { Key = "subject", Header = "Subject", HeaderKhmer = "មុខវិជ្ជា", Width = 150 },
                new() { Key = "exam", Header = "Exam", HeaderKhmer = "ប្រលង", Width = 120 },
                new() { Key = "className", Header = "Class", HeaderKhmer = "ថ្នាក់", Width = 150 },
            };

            // Add dynamic component columns
            foreach (var componentKey in allComponentKeys)
            {
                columns.Add(new ReportColumn
                {
                    Key = $"component_{componentKey}",
                    Header = componentKey,
                    DataType = typeof(decimal),
                    Width = 80,
                });
            }

            columns.Add(new ReportColumn
            {
                Key = "totalScore",
                Header = "Total Score",
                HeaderKhmer = "ពិន្ទុសរុប",
                DataType = typeof(decimal),
                Width = 120,
            });

            return new ReportResult
            {
                Title = "របាយការណ៍ពិន្ទុប្រលង",
                SubTitle = "Score Report",
                GeneratedDate = DateTime.UtcNow,
                Columns = columns,
                Rows = rows,
                Summary = new Dictionary<string, object>
                {
                    ["totalAssessments"] = rows.Count,
                    ["averageScore"] = rows.Count > 0
                        ? Math.Round(rows.Average(r => Convert.ToDecimal(r.GetValueOrDefault("totalScore") ?? 0)), 2)
                        : 0m,
                }
            };
        }
    }
}
