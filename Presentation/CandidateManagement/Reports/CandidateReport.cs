using ClosedXML.Excel;
using KhmerCalendar;
using Microsoft.Win32;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Extensions;
using System.Windows;

namespace CandidateManagement.Reports
{
    public class CandidateReport
    {
        private const string BaseWorksheetName = "Computer";
        private const int TitleRow = 6;
        private const int StudyYearRow = 7;
        private const int LunarDateRow = 12;
        private const int GregorianDateRow = 13;
        private const int ReportDateColumn = 7;
        private const int SummaryFallbackRow = 11;
        private const int SummaryTextColumn = 1;
        private const int SummaryCountColumn = 2;

        private readonly DateTime _reportDate;
        private readonly int _studyYearStart;
        private readonly int _studyYearEnd;
        private readonly IEnumerable<Candidate> _candidates;
        public string TemplatePath { get; set; } = @".\Sources\Spreadsheets\candidate_list.xlsx";
        public string OutputPath { get; private set; } = @"";
        public string FileName { get; private set; } = @"";
        public CandidateReport(DateTime? date, int? startYear, int? endYear, IEnumerable<Candidate> candidates)
        {
            _reportDate = date ?? DateTime.Now;
            _studyYearStart = startYear ?? (DateTime.Now.Month >= 9 ? DateTime.Now.Year : DateTime.Now.Year - 1);
            _studyYearEnd = endYear ?? (DateTime.Now.Month >= 9 ? DateTime.Now.Year + 1 : DateTime.Now.Year);
            _candidates = candidates ?? [];
        }

        public Status GenerateReport()
        {
            XLWorkbook workbook = new(TemplatePath);
            var skillGroups = _candidates
                .Where(c => c.Skill != null && !string.IsNullOrWhiteSpace(c.Skill.Name))
                .GroupBy(c => c.Skill!.Name);

            foreach (var skillGroup in skillGroups)
            {
                string sheetName = skillGroup.Key;
                EnsureWorksheetExists(workbook, sheetName);
            }

            foreach (var skillGroup in skillGroups)
            {
                Candidate[] candidates = skillGroup.ToArray();

                string sheetName = skillGroup.Key;
                Skill studentSkill = candidates[0].Skill!;

                if (candidates.Length <= 0) continue;

                IXLWorksheet worksheet = workbook.Worksheet(sheetName);

                // Update title skill
                UpdateTitleSkill(worksheet, studentSkill);
                // Update study year
                UpdateStudyYear(worksheet, _studyYearStart, _studyYearEnd);
                // Update report date
                UpdateReportDate(worksheet, _reportDate);

                for (int i = 0; i < candidates.Length; i++)
                {
                    int id = i + 1;
                    InsertStudent(candidates.ElementAt(i), worksheet, id);
                }
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Save Generated Report",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"StudentReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                OutputPath = System.IO.Path.GetDirectoryName(saveFileDialog.FileName)!;
                FileName = System.IO.Path.GetFileName(saveFileDialog.FileName);

                try
                {
                    workbook.SaveAs(saveFileDialog.FileName);
                    return Status.Success;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Report failed to generate\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return Status.Failed;
                }
            }
            else
            {
                return Status.Rejected;
            }
        }
        private static void InsertStudent(Candidate student, IXLWorksheet ws, int index)
        {
            int summaryRow = FindSummaryRow(ws);
            if (summaryRow == -1)
                throw new InvalidOperationException("Summary row not found.");

            int newRow = summaryRow; // Insert before summary
            ws.Row(summaryRow).InsertRowsAbove(1);

            // Format cells
            ws.Row(newRow).Height = 18;
            ws.Row(newRow).Unhide();
            ws.Cell(newRow, 5).Style.DateFormat.Format = "[$-km-KH,1200]dd mmmm yyyy;@";
            ws.Cell(newRow, 8).Style.DateFormat.Format = "[$-km-KH,1200]dd mmmm yyyy;@";

            // Fill cells
            ws.Cell(newRow, 1).Value = index; // Serial number
            ws.Cell(newRow, 2).Value = student.LastName;
            ws.Cell(newRow, 3).Value = student.FirstName;
            ws.Cell(newRow, 4).Value = student.Gender.GetDescription(); // "ប្រុស"/"ស្រី"
            ws.Cell(newRow, 5).Value = student.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
            ws.Cell(newRow, 6).Value = student.Age;
            ws.Cell(newRow, 7).Value = student.ExamCenter;
            ws.Cell(newRow, 8).Value = student.ExamDate?.ToDateTime(TimeOnly.MinValue);
            ws.Cell(newRow, 9).Value = student.ExamTable;
            ws.Cell(newRow, 10).Value = student.ExamRoom;
            ws.Cell(newRow, 11).Value = student.FromSchool;
            ws.Cell(newRow, 12).Value = student.OtherInfo;

            // Update summary row totals
            UpdateSummary(ws, summaryRow + 1, student);
        }
        private static void EnsureWorksheetExists(XLWorkbook workbook, string sheetName)
        {
            if (workbook.TryGetWorksheet(sheetName, out _))
            {
                return;
            }

            IXLWorksheet templateSheet = workbook.TryGetWorksheet(BaseWorksheetName, out IXLWorksheet? baseSheet)
                ? baseSheet
                : workbook.Worksheets.First();
            templateSheet.CopyTo(sheetName);
        }
        private static int FindSummaryRow(IXLWorksheet ws)
        {
            foreach (IXLRow row in ws.RowsUsed())
            {
                string? text = row.Cell(SummaryTextColumn).GetString();
                if (!string.IsNullOrWhiteSpace(text) && text.Contains("បញ្ឈប់បញ្ជីត្រឹមឈ្មោះ"))
                {
                    return row.RowNumber();
                }
            }

            return SummaryFallbackRow;
        }
        private static void UpdateSummary(IXLWorksheet ws, int summaryRow, Candidate student)
        {
            IXLCell summaryCell = ws.Cell(summaryRow, SummaryTextColumn);
            IXLCell totalCell = ws.Cell(summaryRow + 1, SummaryCountColumn);
            IXLCell femaleCell = ws.Cell(summaryRow + 2, SummaryCountColumn);

            int total = 0;
            int female = 0;

            try
            {
                total = int.Parse(totalCell.GetString());
                female = int.Parse(femaleCell.GetString());
            }
            catch
            {
                total = 0;
                female = 0;
            }

            total++;
            if (student.Gender == Gender.Female) female++;

            totalCell.Value = total;
            femaleCell.Value = female;

            string newSummary = $"បញ្ឈប់បញ្ជីត្រឹមឈ្មោះ {student.FullName}   ចំនួនសិស្សសរុប {total}នាក់ នៅក្នុងនោះមានស្រី {female}នាក់។".UseKhmerNumbers();
            summaryCell.Value = newSummary;
        }
        private static void UpdateReportDate(IXLWorksheet ws, DateTime date)
        {
            string lunarReportDate = ToReportLunarDate(date);
            string gregorianReportDate = ToReportGregorianDate(date);
            ws.Cell(LunarDateRow, ReportDateColumn).Value = lunarReportDate;
            ws.Cell(GregorianDateRow, ReportDateColumn).Value = gregorianReportDate;
        }
        private static void UpdateStudyYear(IXLWorksheet ws, int startYear, int endYear)
        {
            ws.Cell(StudyYearRow, SummaryTextColumn).Value = $"ឆ្នាំសិក្សា {startYear}-{endYear}".UseKhmerNumbers();
        }
        private static void UpdateTitleSkill(IXLWorksheet ws, Skill skill)
        {
            ws.Cell(TitleRow, SummaryTextColumn).Value = $"បញ្ជីរាយនាមសិស្សចូលរៀនបច្ចេកទេស កម្រិត១ ផ្នែក{skill.KhmerName}";
        }

        private static string ToReportLunarDate(DateTime date)
        {
            string weekDay = ((int)date.DayOfWeek).UseKhmerDays();
            IKhmerLunarDate khmerLunar = date.ToKhmerLunarDate();
            string day = khmerLunar.LunarDay.UseKhmerNumbers();
            string month = khmerLunar.LunarMonth;
            string year = khmerLunar.LunarYear.UseKhmerNumbers();
            string zodiac = khmerLunar.ZodiacYear;
            string stem = khmerLunar.Stem;
            return $"ថ្ងៃ{weekDay}  {day}   ខែ{month}    ឆ្នាំ{zodiac}  {stem}  ព.ស.{year}";
        }
        private static string ToReportGregorianDate(DateTime date)
        {

            string day = date.Day.UseKhmerNumbers();
            string month = date.Month.UseKhmerMonths();
            string year = date.Year.UseKhmerNumbers();
            return $"   ដុនបូស្កូប៉ោយប៉ែត ថ្ងៃទី{day}   ខែ{month}    ឆ្នាំ{year}";
        }

    }
}
