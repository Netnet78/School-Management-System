using ClosedXML.Excel;
using Microsoft.Win32;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Models;
using System.Windows;
using KhmerCalendar;

namespace New_Student_Management.Reports
{
    public class CandidateReport
    {
        private readonly DateTime _reportDate;
        private readonly int _studyYearStart;
        private readonly int _studyYearEnd;
        private readonly List<Skill> _skills;
        public string TemplatePath { get; set; } = @".\Sources\Spreadsheets\candidate_list.xlsx";
        public string OutputPath { get; private set; } = @"";
        public string FileName { get; private set; } = @"";
        public CandidateReport(DateTime? date, int? startYear, int? endYear, List<Skill> skills)
        {
            _reportDate = date ?? DateTime.Now;
            _studyYearStart = startYear ?? (DateTime.Now.Month >= 9 ? DateTime.Now.Year : DateTime.Now.Year - 1);
            _studyYearEnd = endYear ?? (DateTime.Now.Month >= 9 ? DateTime.Now.Year + 1 : DateTime.Now.Year);
            _skills = skills;
        }

        public ReturnStatus GenerateReport()
        {
            XLWorkbook workbook = new(TemplatePath);

            List<Skill> studentsBySkill = _skills;

            foreach (Skill skillGroup in studentsBySkill)
            {
                string sheetName = skillGroup.Name;
                List<Candidate> studentsInSkill = skillGroup.Students.ToList();

                if (studentsInSkill.Count <= 0) continue;

                EnsureWorksheetExists(workbook, sheetName); 

                for (int i = 0; i < studentsInSkill.Count; i++)
                {
                    int id = i + 1;
                    InsertStudent(workbook, studentsInSkill[i], sheetName, id);
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
                    return ReturnStatus.Success;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Report failed to generate\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return ReturnStatus.Failed;
                }
            }
            else
            {
                return ReturnStatus.Rejected;
            }
        }
        private void InsertStudent(XLWorkbook workbook, Candidate student, string sheetName, int index)
        {
            var ws = workbook.Worksheet(sheetName);

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

            // Update study year
            UpdateStudyYear(ws, _studyYearStart, _studyYearEnd);
            // Update summary row totals
            UpdateSummary(ws, summaryRow + 1, student);
            // Update report date
            UpdateReportDate(ws, _reportDate);
        }
        private void EnsureWorksheetExists(XLWorkbook workbook, string sheetName)
        {
            if (workbook.TryGetWorksheet(sheetName, out _))
            {
                return;
            }

            IXLWorksheet templateSheet = workbook.Worksheets.First();
            IXLWorksheet newSheet = workbook.Worksheets.Add(sheetName);

            foreach (IXLRow row in templateSheet.Rows())
            {
                IXLRow newRow = newSheet.Row(row.RowNumber());
                newRow.Height = row.Height;

                foreach (IXLCell cell in row.Cells())
                {
                    IXLCell newCell = newRow.Cell(cell.Address.ColumnNumber);
                    newCell.Value = cell.Value;

                    if (cell.HasFormula)
                    {
                        newCell.FormulaA1 = cell.FormulaA1;
                    }
                }
            }

            var templateColumns = templateSheet.Columns();
            foreach (var templateColumn in templateColumns)
            {
                newSheet.Column(templateColumn.ColumnNumber()).Width = templateColumn.Width;
            }
        }
        private static int FindSummaryRow(IXLWorksheet ws)
        {
            var namedRange = ws.DefinedName("SummaryCell");
            if (namedRange == null) return -1;
            return namedRange.Ranges.First().FirstCell().Address.RowNumber;
        }
        private static void UpdateSummary(IXLWorksheet ws, int summaryRow, Candidate student)
        {
            var summaryCell = ws.Row(summaryRow).CellsUsed().FirstOrDefault() ?? ws.Cell(summaryRow, 1);

            // Extract totals
            var totalCell = ws.DefinedName("TotalStudents").Ranges.First().FirstCell();
            var femaleCell = ws.DefinedName("TotalFemaleStudents").Ranges.First().FirstCell();

            int total = 0;
            int female = 0;

            try
            {
                total = int.Parse(totalCell.Value.ToString());
                female = int.Parse(femaleCell.Value.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error when trying to convert the values into integers\nTotal value = {total}\nFemale total value = {female}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show(ex.Message);
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
            XLWorkbook workbook = ws.Workbook;
            IXLDefinedName lunarDateRange = workbook.DefinedName("LunarReportDate")!;
            if (lunarDateRange != null)
            {
                foreach (IXLRange cell in lunarDateRange.Ranges)
                {
                    cell.Value = lunarReportDate;
                }
            }
            IXLDefinedName gregorianDateRange = workbook.DefinedName("GregorianReportDate")!;
            if (gregorianDateRange != null)
            {
                foreach (IXLRange cell in gregorianDateRange.Ranges)
                {
                    cell.Value = gregorianReportDate;
                }
            }
        }
        private static void UpdateStudyYear(IXLWorksheet ws, int startYear, int endYear)
        {
            XLWorkbook workbook = ws.Workbook;
            IXLDefinedName studyYearRange = workbook.DefinedName("StudyYear")!;
            if (studyYearRange != null)
            {
                foreach (IXLRange cell in studyYearRange.Ranges)
                {
                    cell.Value = $"ឆ្នាំសិក្សា {startYear}-{endYear}".UseKhmerNumbers();
                }
            }
        }

        private static string ToReportLunarDate(DateTime date)
        {
            string weekDay = ((int)date.DayOfWeek).UseKhmerDays();
            IKhmerLunarDate khmerLunar = date.ToKhmerLunarDate();
            string day = khmerLunar.LunarDay;
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