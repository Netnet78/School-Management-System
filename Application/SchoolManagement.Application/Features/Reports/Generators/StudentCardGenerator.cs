using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Helpers;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Assets;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Enums;
using KhmerCalendar;

namespace SchoolManagement.Application.Features.Reports.Generators
{
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

        private static readonly string SealPath = Path.Combine(
            ResourcePaths.Images,
            "dbs_red.png");

        private static byte[]? _sealBytes;

        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IPhotoFetchService _photoFetchService;

        public ReportTag ReportTypeKey => ReportTag.StudentCard;

        public StudentCardGenerator(
            IStudentClassRepository studentClassRepository,
            IPhotoFetchService photoFetchService)
        {
            _studentClassRepository = studentClassRepository;
            _photoFetchService = photoFetchService;

            ConfigureFonts();
        }

        public static void ConfigureFonts()
        {
            QuestPDF.Settings.FontDiscoveryPaths.Clear();
            QuestPDF.Settings.FontDiscoveryPaths.Add(ResourcePaths.Fonts);
        }

        public object CreateDefaultFilter() => new StudentCardFilter
        {
            Location = "វិ.ច.ប.ឯ.ដប.ប៉ប៉",
        };

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var cardFilter = (StudentCardFilter)filter;
            string location = NormalizeLocation(cardFilter.Location);

            var filters = new List<FilterCondition<StudentClass>>();

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
                        ReportTag = ReportTag.StudentCard,
                        CardGroups = [],
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

            if (cardFilter.ClassId.HasValue)
                filters.Add(new FilterCondition<StudentClass>(
                    sc => sc.ClassId, FilterOperator.Equals, cardFilter.ClassId.Value));

            if (!string.IsNullOrWhiteSpace(cardFilter.SearchKeyword))
                filters.Add(new FilterCondition<StudentClass>(
                    sc => sc.Student.Candidate.FullName, FilterOperator.Contains, cardFilter.SearchKeyword));

            int? page = null;
            int? pageSize = null;

            var studentClasses = (await _studentClassRepository.FindAsync(
                filters,
                page: page,
                pageSize: pageSize,
                orderBy: [new SortCriteria<StudentClass>(sc => sc.Student.Candidate.FullName, OrderDirection.Ascending)],
                "Student.Candidate",
                "Student.Candidate.Photo",
                "Student.StudentQR",
                "Class.Grade",
                "Class.Generation.Department").ConfigureAwait(false)).ToList();

            int totalCount = pageSize.HasValue
                ? await _studentClassRepository.CountAsync(filters).ConfigureAwait(false)
                : studentClasses.Count;

            byte[]? sealBytes = GetSealBytes();
            byte[]? signatureBytes = await ReadBytesIfExistsAsync(cardFilter.SignaturePath, cancellationToken).ConfigureAwait(false);

            var photoTasks = studentClasses
                .Where(sc => sc.Student != null && !string.IsNullOrWhiteSpace(sc.Student.PhotoKey))
                .Select(async sc =>
                {
                    var result = await _photoFetchService.GetStudentPhoto(
                        sc.Student.PhotoKey,
                        cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (result.Status == Status.Success && result.Value != null && File.Exists(result.Value.FilePath))
                    {
                        var bytes = await File.ReadAllBytesAsync(result.Value.FilePath, cancellationToken).ConfigureAwait(false);
                        return (Id: sc.Student.Id, Bytes: bytes);
                    }
                    return (Id: sc.Student.Id, Bytes: (byte[]?)null);
                });

            var photoResults = await Task.WhenAll(photoTasks).ConfigureAwait(false);
            var photoMap = new Dictionary<int, byte[]?>(studentClasses.Count);
            foreach (var (id, bytes) in photoResults)
                photoMap[id] = bytes;

            var cardGroups = new List<ReportItemGroup>();

            foreach (var studentClass in studentClasses)
            {
                var student = studentClass.Student;
                var candidate = student?.Candidate;
                var @class = studentClass.Class;

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
                string studentId = $"SDBS{student.Id:0000}";
                string dateOfBirth = candidate.DateOfBirth.HasValue
                    ? ReportDateHelper.FormatDateOfBirth(candidate.DateOfBirth.Value)
                    : "-";
                string birthProvince = SafeText(candidate.BirthProvince);
                string fatherName = SafeText(candidate.FatherName);
                string motherName = SafeText(candidate.MotherName);
                string grade = SafeText($"{@class.Grade.KhmerName.Last()}");
                string department = SafeText(@class.Generation.Department?.KhmerName ?? @class.Generation.Department?.Name);
                string lunarDate = ReportDateHelper.FormatKhmerLunarDate(DateTime.Now);
                string gregorianDate = ReportDateHelper.FormatGregorianDateLine(location, DateTime.Now);

                var items = new List<CardItem>();

                AddText(items, 351, 330, academicYear, 23.75f, false, AccentBlue, null, null, KhmerOSMuolLight, TextAlignment.Center, "AcademicYear");
                AddText(items, 286, 395, fullName, 27.08f, false, AccentRed, null, textAlignment: TextAlignment.Left, fieldName: "FullName", fontFamily: KhmerOSMuolLight);

                AddText(items, 109, 460, gender, 25f, false, AccentBlue, 60, fieldName: "Gender");
                AddText(items, 286, 460, national, 25f, false, AccentBlue, 60);
                AddText(items, 514, 460, studentId, 26.25f, false, null, 100, null, TimesNewRoman, fieldName: "StudentId");

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
                        Value = photoBytes,
                        Width = 139,
                        Height = 180,
                        FieldName = "Photo",
                    });
                }

                if (sealBytes != null)
                {
                    items.Add(new CardItem
                    {
                        XPos = 155,
                        YPos = 670,
                        Value = sealBytes,
                    });
                }

                if (signatureBytes != null)
                {
                    items.Add(new CardItem
                    {
                        XPos = 358,
                        YPos = 842,
                        Value = signatureBytes,
                        Height = 72,
                    });
                }

                if (!string.IsNullOrWhiteSpace(cardFilter.PrincipalName))
                {
                    AddText(
                        items,
                        487,
                        875,
                        cardFilter.PrincipalName.Trim(),
                        25f,
                        false,
                        AccentRed,
                        190,
                        null,
                        KhmerOSMuol);
                }

                cardGroups.Add(new ReportItemGroup
                {
                    Width = CardWidth,
                    Height = CardHeight,
                    TemplateFileFilePath = _templatePath,
                    Items = items,
                });
            }

            return new CardReportResult
            {
                Title = "ប័ណ្ណសម្គាល់សិស្ស",
                SubTitle = "Student Name Cards",
                GeneratedDate = DateTime.Now,
                ReportTag = ReportTag.StudentCard,
                CardGroups = cardGroups,
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
            if (_sealBytes != null)
            {
                return _sealBytes;
            }

            if (!File.Exists(SealPath))
            {
                return null;
            }

            _sealBytes = File.ReadAllBytes(SealPath);
            return _sealBytes;
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
