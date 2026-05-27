using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Export;
using System.Diagnostics;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    public class StudentCardGenerator : IReportGenerator
    {
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IPhotoFetchService _photoFetchService;

        public string ReportTypeKey => "student-card";

        public StudentCardGenerator(
            IStudentClassRepository studentClassRepository,
            IPhotoFetchService photoFetchService)
        {
            _studentClassRepository = studentClassRepository;
            _photoFetchService = photoFetchService;
        }

        public object CreateDefaultFilter() => new StudentCardFilter();

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var cardFilter = (StudentCardFilter)filter;

            var filters = new List<FilterCondition<StudentClass>>();

            if (cardFilter.ClassId.HasValue)
                filters.Add(new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, cardFilter.ClassId.Value));

            var studentClasses = await _studentClassRepository.FindAsync(
                filters,
                page: null,
                pageSize: null,
                orderBy: new[] { new SortCriteria<StudentClass>(sc => sc.Student.Candidate.FullName, OrderDirection.Ascending) },
                "Student.Candidate", "Student.StudentQR", "Class.Grade", "Class.Generation.Department");

            var cardGroups = new List<ReportItemGroup>();
            const int cardWidth = 709;
            const int cardHeight = 945;

            foreach (var sc in studentClasses)
            {
                var student = sc.Student;
                var candidate = student?.Candidate;
                var @class = sc.Class;

                byte[]? photoBytes = null;
                if (!string.IsNullOrWhiteSpace(student?.PhotoKey))
                {
                    var photoResult = await _photoFetchService.GetStudentPhoto(student.PhotoKey, cancellationToken: cancellationToken);
                    if (photoResult.Status == Status.Success && photoResult.Value != null)
                    {
                        photoBytes = File.Exists(photoResult.Value.FilePath)
                            ? await File.ReadAllBytesAsync(photoResult.Value.FilePath, cancellationToken)
                            : null;
                    }
                }

                var fullName = candidate?.FullName ?? "";
                var latinName = candidate?.LatinFullName ?? "";
                var gender = candidate?.Gender.ToString() ?? "";
                var className = @class?.KhmerName ?? "";

                byte[]? qrBytes = null;
                var (qrWidth, qrHeight) = (300, 300);
                var qrValue = student?.StudentQR?.QRCodeValue;
                if (!string.IsNullOrEmpty(qrValue))
                {
                    qrBytes = QRCodeExporter.GenerateQrPng(qrValue, qrWidth, qrHeight);
                }

                // Front card: photo + info
                var frontItems = new List<ReportItem>();

                if (photoBytes != null)
                {
                    frontItems.Add(new ReportItem { XPos = 5, YPos = 5, Value = photoBytes });
                }

                frontItems.Add(new ReportItem { XPos = 85, YPos = 5, Value = fullName, FontSize = 11, IsBold = true });
                frontItems.Add(new ReportItem { XPos = 85, YPos = 30, Value = latinName, FontSize = 9, FontColor = "#757575" });
                frontItems.Add(new ReportItem { XPos = 85, YPos = 55, Value = className, FontSize = 9 });
                frontItems.Add(new ReportItem { XPos = 85, YPos = 80, Value = gender, FontSize = 9, FontColor = "#9E9E9E" });

                cardGroups.Add(new ReportItemGroup
                {
                    Width = cardWidth,
                    Height = cardHeight,
                    Items = frontItems
                });

                var backItems = new List<ReportItem>();

                if (qrBytes != null)
                {
                    backItems.Add(new ReportItem 
                    {
                        XPos = (cardWidth - qrWidth) / 2, 
                        YPos = (cardHeight - qrHeight) / 2, 
                        Value = qrBytes
                    });
                }

                cardGroups.Add(new ReportItemGroup
                {
                    Width = cardWidth,
                    Height = cardHeight,
                    Items = backItems
                });
            }

            return new ReportResult
            {
                Title = "ប័ណ្ណសិស្សានុសិស្ស",
                SubTitle = "Student Name Cards",
                GeneratedDate = DateTime.UtcNow,
                Layout = ReportLayout.Card,
                Columns =
                [
                    new() { Key = "fullName", Header = "Full Name", HeaderKhmer = "ឈ្មោះពេញ", Width = 200 },
                    new() { Key = "latinFullName", Header = "Latin Name", HeaderKhmer = "ឈ្មោះឡាតាំង", Width = 180 },
                    new() { Key = "gender", Header = "Gender", HeaderKhmer = "ភេទ", Width = 80 },
                    new() { Key = "className", Header = "Class", HeaderKhmer = "ថ្នាក់", Width = 200 },
                ],
                CardGroups = cardGroups,
                Summary = new Dictionary<string, object>
                {
                    ["totalStudents"] = studentClasses.Count(),
                }
            };
        }
    }
}
