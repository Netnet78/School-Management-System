using KhmerCalendar;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Assets;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    public class StudentRosterGenerator : IReportGenerator
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IClassRepository _classRepository;
        private readonly IAuthorizationService _authorizationService;

        private readonly string _templatePath = Path.Combine(ResourcePaths.Spreadsheets, "student_roster.xlsx");

        public ReportTag ReportTypeKey => ReportTag.StudentRoster;

        public StudentRosterGenerator(
            IStudentClassRepository studentClassRepository,
            IAuthorizationService authorizationService,
            ISkillRepository skillRepository,
            IClassRepository classRepository)
        {
            _studentClassRepository = studentClassRepository;
            _authorizationService = authorizationService;
            _skillRepository = skillRepository;
            _classRepository = classRepository;
        }

        public object CreateDefaultFilter() => new StudentRosterFilter
        {
            IsActive = true
        };

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var studentFilter = (StudentRosterFilter)filter;

            var filters = new List<FilterCondition<StudentClass>>();

            if (studentFilter.IsActive.HasValue)
                filters.Add(new FilterCondition<StudentClass>(sc => sc.Student.IsActive, FilterOperator.Equals, studentFilter.IsActive.Value));

            if (!string.IsNullOrWhiteSpace(studentFilter.SearchKeyword))
                filters.Add(new FilterCondition<StudentClass>(sc => sc.Student.Candidate.FullName, FilterOperator.Contains,
                    studentFilter.SearchKeyword));

            if (studentFilter.EnrollDateFrom.HasValue)
                filters.Add(new FilterCondition<StudentClass>(sc => sc.Student.EnrollDate, FilterOperator.GreaterThanOrEqual,
                    studentFilter.EnrollDateFrom.Value));

            if (studentFilter.EnrollDateTo.HasValue)
                filters.Add(new FilterCondition<StudentClass>(sc => sc.Student.EnrollDate, FilterOperator.LessThanOrEqual,
                    studentFilter.EnrollDateTo.Value));

            var studentClasses = await _studentClassRepository.FindAsync(
                filters,
                orderBy: [new SortCriteria<StudentClass>(sc => sc.Student.Candidate.FullName, OrderDirection.Ascending)],
                includes: ["Student.Candidate", "Class.Grade", "Class.Generation.Department", "Class.Generation.Department.Skill"])
                .ConfigureAwait(false);

            User? user = _authorizationService.CurrentUser;
            bool isAdmin = user?.IsAdmin() == true;
            bool isHeadTeacher = user?.IsHeadTeacher() == true;

            var skillGroups = studentClasses
                .GroupBy(sc => sc.Student.SkillId)
                .Select(skillGroup => new
                {
                    SkillId = skillGroup.Key,
                    Classes = skillGroup
                        .GroupBy(sc => sc.ClassId)
                        .OrderBy(g => g.First().Class.Generation.Department.Id)
                });

            List<TableReportGroup> tableReportGroups = [];

            foreach (var group in skillGroups)
            {
                foreach (var studentClassGroup in group.Classes)
                {
                    Class? @class = await _classRepository.GetByIdWithSubjectsAsync(studentClassGroup.Key);
                    var rows = new List<Dictionary<string, ReportCell>>();

                    foreach (StudentClass sc in studentClassGroup)
                    {
                        Student? student = sc.Student;
                        Candidate? candidate = student?.Candidate;

                        var row = new Dictionary<string, ReportCell>
                        {
                            ["fullName"] = candidate?.FullName,
                            ["latinFullName"] = candidate?.LatinFullName,
                            ["gender"] = candidate?.Gender.ToString(),
                            ["dateOfBirth"] = candidate?.DateOfBirth?.ToString("dd/MM/yyyy"),
                            ["status"] = student?.IsActiveReadable,
                            ["enrollDate"] = student?.EnrollDate?.ToString("dd/MM/yyyy"),
                            ["placeOfBirth"] = candidate?.BirthProvince,
                            ["fatherName"] = candidate?.FatherName,
                            ["motherName"] = candidate?.MotherName,
                            ["phoneNumber"] = candidate?.PhoneNumber,
                            ["examCenter"] = candidate?.ExamCenter,
                            ["examDate"] = candidate?.ExamDate.ToString(),
                            ["examRoom"] = candidate?.ExamRoom.ToString(),
                            ["examTable"] = candidate?.ExamTable.ToString(),
                        };

                        if (isAdmin)
                        {
                            row["grade"] = @class?.Grade?.KhmerName;
                            row["department"] = candidate?.Skill?.KhmerName ?? @class?.Generation?.Department?.KhmerName;
                        }
                        else if (isHeadTeacher)
                        {
                            row["grade"] = @class?.Grade?.KhmerName;
                        }
                        else
                        {
                            row["className"] = @class?.KhmerName;
                        }

                        rows.Add(row);
                    }

                    var columns = new List<ReportColumn>
                    {
                        new() { Key = "fullName", Header = "Full Name", HeaderKhmer = "ឈ្មោះពេញ", Width = 200 },
                        new() { Key = "latinFullName", Header = "Latin Name", HeaderKhmer = "ឈ្មោះឡាតាំង", Width = 180 },
                        new() { Key = "gender", Header = "Gender", HeaderKhmer = "ភេទ", Width = 80 },
                        new() { Key = "dateOfBirth", Header = "DOB", HeaderKhmer = "ថ្ងៃខែឆ្នាំកំណើត", Width = 120 },
                    };

                    if (isAdmin)
                    {
                        columns.Add(new() { Key = "grade", Header = "Grade", HeaderKhmer = "កម្រិត", Width = 100 });
                        columns.Add(new() { Key = "department", Header = "Department", HeaderKhmer = "ជំនាញ", Width = 120 });
                    }
                    else if (isHeadTeacher)
                    {
                        columns.Add(new() { Key = "grade", Header = "Grade", HeaderKhmer = "កម្រិត", Width = 100 });
                    }
                    else
                    {
                        columns.Add(new() { Key = "className", Header = "Class", HeaderKhmer = "ថ្នាក់", Width = 200 });
                    }

                    columns.Add(new() { Key = "status", Header = "Status", HeaderKhmer = "ស្ថានភាព", Width = 100 });
                    columns.Add(new() { Key = "enrollDate", Header = "Enroll Date", HeaderKhmer = "ថ្ងៃចុះឈ្មោះ", Width = 120 });

                    Skill skill = await _skillRepository.GetByIdAsync(group.SkillId)
                        ?? throw new InvalidOperationException("Student's skill cannot be null when generating a student roster!");

                    TableReportGroup result = new()
                    {
                        Columns = columns,
                        Rows = rows,
                        Title = $"បញ្ជីរាយនាមសិស្សានុសិស្សផ្នែក {skill.KhmerName} " +
                        $"{@class?.Grade.KhmerName} ឆ្នាំសិក្សា {studentFilter.StartYear}-{studentFilter.EndYear}".UseKhmerNumbers(),
                        Name = $"{@class?.Department.Name} {@class?.Grade.Name}",
                        KhmerName = $"{@class?.Department.KhmerName} {@class?.Grade.KhmerName}",
                        Summary = new()
                        {
                            ["សិស្សសរុប"] = rows.Count,
                            ["ក្នុងនោះមានស្រី"] = rows.Count(r => (string?)r["gender"].Value == "Female")
                        }
                    };

                    tableReportGroups.Add(result);
                }
            }

            return new GroupedTableReportResult()
            {
                ReportTag = ReportTag.StudentRoster,
                Groups = tableReportGroups,
                GeneratedDate = DateTime.Now,
                ReportDate = studentFilter.ReportDate.ToDateTime(TimeOnly.MinValue),
                Title = $"បញ្ជីរាយនាមសិស្សានុសិស្សតាមផ្នែកនីមួយៗ" +
                $"នៅក្នុងឆ្នាំសិក្សា {studentFilter.StartYear}-{studentFilter.EndYear}".UseKhmerNumbers(),
                TemplatePath = _templatePath
            };
        }
    }
}
