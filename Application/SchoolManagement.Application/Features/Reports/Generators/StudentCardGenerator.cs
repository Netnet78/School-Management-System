using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Helpers;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Assets;
using SchoolManagement.Core.Features.Reports.Attributes;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    [ReportType(Key = "student-card", DisplayName = "Student Name Cards", DisplayNameKhmer = "ប័ណ្ណសិស្ស",
        Description = "Generate printable student name cards with photo, seal overlay, and editable signature", IconKind = "CardAccountDetails", SortOrder = 4, ReportStyle = ReportStyle.Card,
        SupportedExportFormats = new[] { "PDF" })]
    public class StudentCardGenerator : IReportGenerator
    {
        private const int CardWidth = 709;
        private const int CardHeight = 945;
        private const string AccentBlue = "#2589d3";
        private const string AccentRed = "#e80808";
        private const string KhmerOSMuolLight = "Khmer OS Muol Light";
        private const string KhmerOSBattambang = "Khmer OS Battambang";
        private const string KhmerOSBokor = "Khmer OS Bokor";
        private const string TimesNewRoman = "Times New Roman";
        private const string KhmerOSMuol = "Khmer OS Muol";

        private readonly string _templatePath = Path.Combine(ResourcePaths.Images, "name_card_template.png");

        private static readonly string _sealPath = Path.Combine(
            ResourcePaths.Images,
            "dbs_red.png");

        private static byte[]? _sealBytes;

        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IAuthorizationService _authorizationService;

        public string ReportTypeKey => "student-card";

        public StudentCardGenerator(
            IStudentClassRepository studentClassRepository,
            IPhotoFetchService photoFetchService,
            IAuthorizationService authorizationService)
        {
            _studentClassRepository = studentClassRepository;
            _photoFetchService = photoFetchService;
            _authorizationService = authorizationService;
        }

        public object CreateDefaultRequest() => new StudentCardReportRequest()
        {
            Options = new StudentCardOptions
            {
                Location = "វិ.ច.ប.ឯ.ដប.ប៉ប៉",
                PrincipalName = "សយ ថាត",
                ReportDate = DateTime.Now
            },
        };

        public async Task<ReportResult> GenerateAsync(object request, CancellationToken cancellationToken = default)
        {
            StudentCardReportRequest cardRequest = (StudentCardReportRequest)request;
            StudentCardFilter cardFilter = cardRequest.Filter;
            StudentCardOptions cardOptions = cardRequest.Options;
            string location = NormalizeLocation(cardOptions.Location);

            List<FilterCondition<StudentClass>> filters = [];

            if (cardFilter.ManualSelectEnabled)
            {
                if (cardFilter.SelectedStudentIds?.Count > 0)
                {
                    filters.Add(new FilterCondition<StudentClass>(
                        sc => sc.StudentId,
                        FilterOperator.In,
                        cardFilter.SelectedStudentIds.Cast<object>()));
                }
                else
                {
                    return new CardReportResult
                    {
                        Title = "ប័ណ្ណសម្គាល់សិស្ស",
                        SubTitle = "Student Name Cards",
                        GeneratedDate = DateTime.Now,
                        ReportTag = "student-card",
                        CardGroups = [],
                        Columns =
                        [
                            new() { Key = "Photo", Header = "Photo", HeaderKhmer = "រូបថត", Width = 80 },
                            new() { Key = "StudentId", Header = "ID", HeaderKhmer = "លេខសម្គាល់", Width = 130 },
                            new() { Key = "FullName", Header = "Full Name", HeaderKhmer = "ឈ្មោះពេញ", Width = 260 },
                            new() { Key = "Gender", Header = "Gender", HeaderKhmer = "ភេទ", Width = 80 },
                            new() { Key = "DateOfBirth", Header = "DOB", HeaderKhmer = "ថ្ងៃខែឆ្នាំកំណើត", Width = 160 },
                            new() { Key = "Class", Header = "Class", HeaderKhmer = "ឆ្នាំទី/កម្រិតទី", Width = 160 },
                            new() { Key = "Department", Header = "Department", HeaderKhmer = "ផ្នែក", Width = 190 },
                            new() { Key = "AcademicYear", Header = "Academic Year", HeaderKhmer = "ឆ្នាំសិក្សា", Width = 170 },
                        ],
                        Layout = new CardSheetLayout
                        {
                            PageSize = "A4",
                            Landscape = true,
                            Columns = 3,
                            Rows = 2,
                            Margin = 14f,
                            HorizontalSpacing = 0f,
                            VerticalSpacing = 0f,
                            ShowHeader = false,
                            ShowFooter = false,
                        },
                        Summary = new Dictionary<string, object>
                        {
                            ["__totalCount"] = 0,
                            ["ចំនួនសិស្សសរុប"] = 0,
                        },
                    };
                }
            }
            else
            {
                filters = BuildFilters(cardFilter);
            }

            var studentClasses = (await _studentClassRepository.FindAsync(
                filters,
                page: cardFilter.Page,
                pageSize: cardFilter.PageSize,
                orderBy: [new SortCriteria<StudentClass>(sc => sc.Student.Candidate.FullName, OrderDirection.Ascending)],
                "Student.Candidate",
                "Student.Candidate.Photo",
                "Student.StudentQR",
                "Class.Grade",
                "Class.Generation.Department").ConfigureAwait(false)).ToList();

            int totalCount = cardFilter.PageSize.HasValue
                ? await _studentClassRepository.CountAsync(filters).ConfigureAwait(false)
                : studentClasses.Count;

            byte[]? sealBytes = GetSealBytes();
            byte[]? signatureBytes = await ReadBytesIfExistsAsync(cardOptions.SignaturePath, cancellationToken).ConfigureAwait(false);

            IEnumerable<Task<(int Id, byte[]? Bytes)>> photoTasks = studentClasses
                .Where(sc => sc.Student != null && !string.IsNullOrWhiteSpace(sc.Student.PhotoKey))
                .Select(async sc =>
                {
                    var result = await _photoFetchService.GetStudentPhoto(
                        sc.Student.PhotoKey,
                        cancellationToken: cancellationToken).ConfigureAwait(false);

                    try
                    {
                        if (result.Status == Status.Success && result.Value != null)
                        {
                            byte[] bytes = await File.ReadAllBytesAsync(result.Value.FilePath, cancellationToken).ConfigureAwait(false);
                            return (Id: sc.Student.Id, Bytes: bytes);
                        }
                        return (Id: sc.Student.Id, Bytes: (byte[]?)null);
                    }
                    catch (FileNotFoundException)
                    {
                        return (Id: sc.StudentId, Bytes: null);
                    }
                    catch (IOException)
                    {
                        return (Id: sc.StudentId, Bytes: null);
                    }
                    catch (Exception) { throw; }
                });

            (int Id, byte[]? Bytes)[] photoResults = await Task.WhenAll(photoTasks).ConfigureAwait(false);
            Dictionary<int, byte[]?> photoMap = new(studentClasses.Count);
            foreach (var (id, bytes) in photoResults)
                photoMap[id] = bytes;

            List<CardDefinition> cardGroups = new();

            foreach (StudentClass studentClass in studentClasses)
            {
                Student student = studentClass.Student;
                Candidate candidate = student.Candidate;
                Class @class = studentClass.Class;

                if (student == null || candidate == null || @class == null)
                {
                    continue;
                }

                byte[]? photoBytes = photoMap.GetValueOrDefault(student.Id);

                string academicYear = ReportDateHelper.FormatAcademicYear(
                    studentClass.StartDate.Year,
                    studentClass.EndDate.Year);

                string fullName = SafeText(candidate.FullName);
                string lastName = SafeText(candidate.LastName);
                string firstName = SafeText(candidate.FirstName);
                string gender = SafeText(candidate.Gender.GetDescription());
                string national = "ខ្មែរ";
                string studentId = $"SDB{student.CandidateId:0000000}";
                string dateOfBirth = candidate.DateOfBirth.HasValue
                    ? ReportDateHelper.FormatDateOfBirth(candidate.DateOfBirth.Value)
                    : "-";
                string birthProvince = SafeText(candidate.BirthProvince);
                string fatherName = SafeText(candidate.FatherName);
                string motherName = SafeText(candidate.MotherName);
                string grade = SafeText($"{@class.Grade.KhmerName.Last()}");
                string department = SafeText(@class.Generation.Department?.KhmerName ?? @class.Generation.Department?.Name);
                string lunarDate = ReportDateHelper.FormatKhmerLunarDate(cardOptions.ReportDate);
                string gregorianDate = ReportDateHelper.FormatGregorianDateLine(location, cardOptions.ReportDate);

                var items = new List<CardItem>();

                AddText(items, 352, 330, academicYear, 23.75f, false, AccentBlue, null, null, KhmerOSMuolLight, TextAlignment.Center, "AcademicYear");
                AddText(items, 286, 395, fullName, 27.08f, false, AccentRed, null, textAlignment: TextAlignment.Left, fieldName: "FullName", fontFamily: KhmerOSMuolLight);

                AddText(items, 109, 460, gender, 25f, false, AccentBlue, 60, fieldName: "Gender");
                AddText(items, 286, 460, national, 25f, false, "#000", 60);
                AddText(items, 510, 460, studentId, 26.25f, false, AccentBlue, 100, null, TimesNewRoman, fieldName: "StudentId");

                AddText(items, 380, 520, dateOfBirth, 25f, false, null, 130, textAlignment: TextAlignment.Center, fieldName: "DateOfBirth");

                AddText(items, 286, 577, "ខេត្ត", 25f, false, null, 60, null, KhmerOSBokor);
                AddText(items, 380, 577, birthProvince, 25f, false, null, 131, null, KhmerOSBokor);

                AddText(items, 205, 634, fatherName, 25f, false, AccentBlue, 120);
                AddText(items, 516, 634, motherName, 25f, false, AccentBlue, 120);

                AddText(items, 207, 696, grade, 25f, false, null, 120, fieldName: "Class");
                AddText(items, 452, 696, department, 25f, false, null, 120, null, KhmerOSBokor, fieldName: "Department");

                AddText(items, 480, 745, lunarDate, 18f, false, null, 200, textAlignment: TextAlignment.Right);
                AddText(items, 480, 781, gregorianDate, 18f, false, null, 200, textAlignment: TextAlignment.Right);

                if (photoBytes != null)
                {
                    items.Add(new CardItem
                    {
                        XPos = 56,
                        YPos = 738,
                        Width = 139,
                        Height = 180,
                        FieldName = "Photo",
                        Value = new BitmapInfo(photoBytes),
                    });
                }

                if (sealBytes != null)
                {
                    items.Add(new CardItem
                    {
                        XPos = 155,
                        YPos = 670,
                        Value = new BitmapInfo(sealBytes),
                    });
                }

                if (signatureBytes != null)
                {
                    items.Add(new CardItem
                    {
                        XPos = 358,
                        YPos = 842,
                        Value = new BitmapInfo(signatureBytes),
                        Height = 68,
                    });
                }

                if (!string.IsNullOrWhiteSpace(cardOptions.PrincipalName))
                {
                    AddText(
                        items,
                        485,
                        895,
                        cardOptions.PrincipalName.Trim(),
                        25f,
                        false,
                        AccentRed,
                        190,
                        null,
                        KhmerOSMuol);
                }

                items.Add(new CardItem
                {
                    FieldName = "__rawId",
                    Value = student.Id,
                });

                cardGroups.Add(new CardDefinition
                {
                    Width = CardWidth,
                    Height = CardHeight,
                    TemplateFilePath = _templatePath,
                    Items = items,
                });
            }

            return new CardReportResult
            {
                Title = "ប័ណ្ណសម្គាល់សិស្ស",
                SubTitle = "Student Name Cards",
                GeneratedDate = DateTime.Now,
                ReportTag = "student-card",
                CardGroups = cardGroups,
                Columns =
                [
                    new() { Key = "Photo", Header = "Photo", HeaderKhmer = "រូបថត", Width = 80 },
                    new() { Key = "StudentId", Header = "ID", HeaderKhmer = "លេខសម្គាល់", Width = 130 },
                    new() { Key = "FullName", Header = "Full Name", HeaderKhmer = "ឈ្មោះពេញ", Width = 260 },
                    new() { Key = "Gender", Header = "Gender", HeaderKhmer = "ភេទ", Width = 80 },
                    new() { Key = "DateOfBirth", Header = "DOB", HeaderKhmer = "ថ្ងៃខែឆ្នាំកំណើត", Width = 160 },
                    new() { Key = "Class", Header = "Class", HeaderKhmer = "ឆ្នាំទី/កម្រិតទី", Width = 160 },
                    new() { Key = "Department", Header = "Department", HeaderKhmer = "ផ្នែក", Width = 150 },
                    new() { Key = "AcademicYear", Header = "Academic Year", HeaderKhmer = "ឆ្នាំសិក្សា", Width = 190 },
                ],
                Layout = new CardSheetLayout
                {
                    PageSize = "A4",
                    Landscape = true,
                    Columns = 3,
                    Rows = 2,
                    Margin = 14f,
                    HorizontalSpacing = 0f,
                    VerticalSpacing = 0f,
                    ShowHeader = false,
                    ShowFooter = false,
                },
                Summary = new Dictionary<string, object>
                {
                    ["__totalCount"] = totalCount,
                    ["ចំនួនសិស្សសរុប"] = studentClasses.Count,
                },
            };
        }

        public async Task<List<int>> GetMatchingStudentIdsAsync(StudentCardFilter cardFilter)
        {
            var filters = BuildFilters(cardFilter);
            var studentClasses = await _studentClassRepository.FindAsync(
                filters, page: null, pageSize: null).ConfigureAwait(false);
            return studentClasses.Select(sc => sc.StudentId).Distinct().ToList();
        }

        private List<FilterCondition<StudentClass>> BuildFilters(StudentCardFilter cardFilter)
        {
            List<FilterCondition<StudentClass>> filters = [];

            if (cardFilter.ClassId.HasValue)
                filters.Add(new FilterCondition<StudentClass>(
                    sc => sc.ClassId, FilterOperator.Equals, cardFilter.ClassId.Value));

            if (!string.IsNullOrWhiteSpace(cardFilter.SearchKeyword))
                filters.Add(new FilterCondition<StudentClass>(
                    sc => sc.Student.Candidate.FullName, FilterOperator.Contains, cardFilter.SearchKeyword));

            User? user = _authorizationService.CurrentUser;

            if (user?.IsHeadTeacher() == true)
            {
                filters.Add(new FilterCondition<StudentClass>(sc => sc.Student.Candidate.SkillId, FilterOperator.Equals,
                    user.Employee?.DepartmentId));
            }
            else if (user?.IsValidRole(RoleType.Teacher) is true)
            {
                filters.Add(new FilterCondition<StudentClass>(sc => sc.Class.TeacherId, FilterOperator.Equals, user.EmployeeId));
            }

            return filters;
        }

        private static void AddText(
            List<CardItem> items,
            int x,
            int y,
            string value,
            float fontSize,
            bool isBold,
            string? fontColor = "#000",
            double? width = null,
            double? height = null,
            string? fontFamily = KhmerOSBattambang, 
            TextAlignment textAlignment = TextAlignment.Left,
            string? fieldName = null)
        {
            items.Add(new CardItem
            {
                XPos = x,
                YPos = y,
                Value = value,
                FontSize = fontSize,
                IsBold = isBold,
                FontColor = fontColor ?? "#000",
                FontFamily = fontFamily,
                Width = width,
                Height = height,
                TextAlignment = textAlignment,
                FieldName = fieldName
            });
        }

        private static string SafeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }

        private static string NormalizeLocation(string? location)
        {
            return string.IsNullOrWhiteSpace(location)
                ? "វិ.ច.ប.ឯ.ដប.ប៉ប៉"
                : location.Trim();
        }

        private static byte[]? GetSealBytes()
        {
            try
            {
                if (_sealBytes != null)
                {
                    return _sealBytes;
                }

                _sealBytes = File.ReadAllBytes(_sealPath);
                return _sealBytes;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            catch (Exception) { throw; }
        }

        private static async Task<byte[]?> ReadBytesIfExistsAsync(string? filePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            return await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
        }
    }
}
