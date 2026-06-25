using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Features.Reports.Attributes;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    [ReportType(Key = "score-report", DisplayName = "Score Report", DisplayNameKhmer = "របាយការណ៍ពិន្ទុ",
        Description = "View and export assessment scores by class, subject, and exam", IconKind = "Scoreboard", SortOrder = 3, ReportStyle = ReportStyle.Table,
        SupportedExportFormats = new[] { "Excel" })]
    public class ScoreReportGenerator : IReportGenerator
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IExamRepository _examRepository;

        public string ReportTypeKey => "score-report";

        public ScoreReportGenerator(
            IAssessmentRepository assessmentRepository,
            IExamRepository examRepository)
        {
            _assessmentRepository = assessmentRepository;
            _examRepository = examRepository;
        }

        public object CreateDefaultRequest() => new ScoreReportFilter();

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var scoreFilter = (ScoreReportFilter)filter;

            var filters = new List<FilterCondition<Assessment>>();

            if (scoreFilter.ExamId.HasValue)
                filters.Add(new FilterCondition<Assessment>(a => a.ExamId, FilterOperator.Equals, scoreFilter.ExamId.Value));

            if (!string.IsNullOrWhiteSpace(scoreFilter.SearchKeyword))
                filters.Add(new FilterCondition<Assessment>(a => a.StudentClass.Student.Candidate.FullName, FilterOperator.Contains,
                    scoreFilter.SearchKeyword));

            var assessments = await _assessmentRepository.FindAsync(
                filters,
                page: null,
                pageSize: null,
                orderBy: new[] { new SortCriteria<Assessment>(a => a.StudentClass.Student.Candidate.FullName, OrderDirection.Ascending) },
                "StudentClass.Student.Candidate",
                "ClassSubject.Subject",
                "ClassSubject.Class",
                "Exam",
                "Scores.Component").ConfigureAwait(false);

            // Filter by class/subject after retrieval
            var filtered = assessments.AsEnumerable();

            if (scoreFilter.ClassId.HasValue)
                filtered = filtered.Where(a => a.ClassSubject?.ClassId == scoreFilter.ClassId.Value);

            if (scoreFilter.SubjectId.HasValue)
                filtered = filtered.Where(a => a.ClassSubject?.SubjectId == scoreFilter.SubjectId.Value);

            var assessmentList = filtered.ToList();
            int totalCount = assessmentList.Count;

            if (scoreFilter.PageSize.HasValue)
            {
                assessmentList = assessmentList
                    .Skip(scoreFilter.PageSize.Value * (scoreFilter.Page - 1))
                    .Take(scoreFilter.PageSize.Value)
                    .ToList();
            }

            // Collect all unique component names across all assessments
            var allComponentKeys = assessmentList
            // TODO: Fix the current implementation to support the actualy component id rather than its mapper
                .SelectMany(a => a.Scores)
                .Where(s => s.Mapper != null)
                .Select(s => s.Mapper.Component)
                .Distinct()
                .Order()
                .ToList();

            var rows = new List<Dictionary<string, ReportCell>>();

            foreach (var assessment in assessmentList)
            {
                var candidate = assessment.StudentClass?.Student?.Candidate;
                var subject = assessment.ClassSubject?.Subject;
                var exam = assessment.Exam;

                var row = new Dictionary<string, ReportCell>
                {
                    ["studentName"] = candidate?.FullName,
                    ["latinName"] = candidate?.LatinFullName,
                    ["subject"] = subject?.KhmerName ?? subject?.Name,
                    ["exam"] = exam?.KhmerName ?? exam?.Name,
                    ["className"] = assessment.ClassSubject?.Class?.KhmerName,
                    ["totalScore"] = assessment.TotalScore,
                };

                // Add component scores as dynamic columns
                foreach (SubjectComponent? component in allComponentKeys)
                {
                    var score = assessment.Scores
                        // TODO: Fix the current implementation to support the actualy component id rather than its mapper
                        .FirstOrDefault(s => s.Mapper?.Id == component.Id);

                    row[$"component_{component}"] = score?.Amount ?? 0;
                }

                rows.Add(row);
            }

            var columns = new List<ReportColumn>
            {
                new() { Key = "studentName", Header = "Student Name", HeaderKhmer = "ឈ្មោះសិស្ស", Width = 200 },
                new() { Key = "latinName", Header = "Latin Name", HeaderKhmer = "ឈ្មោះឡាតាំង", Width = 180 },
                new() { Key = "subject", Header = "Subject", HeaderKhmer = "មុខវិជ្ជា", Width = 150 },
                new() { Key = "exam", Header = "Exam", HeaderKhmer = "ប្រលង", Width = 160 },
                new() { Key = "className", Header = "Class", HeaderKhmer = "ថ្នាក់", Width = 200 },
            };

            // Add dynamic component columns
            foreach (SubjectComponent? component in allComponentKeys)
            {
                columns.Add(new ReportColumn
                {
                    Key = $"component_{component.Name}",
                    Header = component.Name,
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

            return new TableReportResult
            {
                ReportTag = "score-report",
                Title = "របាយការណ៍ពិន្ទុប្រលង",
                SubTitle = "Score Report",
                GeneratedDate = DateTime.UtcNow,
                Columns = columns,
                Rows = rows,
                Summary = new Dictionary<string, object>
                {
                    ["__totalCount"] = totalCount,
                    ["ចំនួនសិស្សានុសិស្ស"] = rows.Count,
                    ["ពិន្ទុមធ្យម"] = rows.Count > 0
                        ? Math.Round(rows.Average(r => Convert.ToDecimal(r.GetValueOrDefault("totalScore")?.Value ?? 0m)), 2)
                        : 0m,
                }
            };
        }
    }
}
