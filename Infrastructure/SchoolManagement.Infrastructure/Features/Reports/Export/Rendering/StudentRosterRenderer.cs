using ClosedXML.Excel;
using KhmerCalendar;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class StudentRosterRenderer : IExcelRenderer
    {
        public bool CanRender(ReportResult result)
        {
            return result.ReportTag == ReportTag.StudentRoster && result is GroupedTableReportResult;
        }

        public XLWorkbook Render(ReportResult data)
        {
            GroupedTableReportResult tableData = (GroupedTableReportResult)data;
            XLWorkbook workbook = new();

            if (!string.IsNullOrWhiteSpace(tableData.TemplatePath))
            {
                if (!File.Exists(tableData.TemplatePath))
                    throw new FileNotFoundException($"Excel template not found: {tableData.TemplatePath}", tableData.TemplatePath);

                workbook = new XLWorkbook(tableData.TemplatePath);
            }

            if (!workbook.TryGetWorksheet("Default", out var defaultWorksheet))
            {
                throw new InvalidOperationException("Couldn't find the 'Default' sheet inside the template file!");
            }

            foreach (TableReportGroup group in tableData.Groups)
            {
                string sheetName = group.KhmerName ?? group.Name;

                // Truncate sheet name to 31 characters since Excel has limitations
                if (sheetName.Length > 31) sheetName = sheetName[..31];

                IXLWorksheet ws = defaultWorksheet.CopyTo(sheetName);

                RenderHeader(ws, group);
                RenderRows(ws, group);
                RenderFooter(ws, group.Rows, tableData);

                ws.Cell("A1").Select();
            }

            if (workbook.Worksheets.Count > 1 && defaultWorksheet != null)
                defaultWorksheet.Delete();

            return workbook;
        }

        private static void RenderHeader(IXLWorksheet ws, TableReportGroup group)
        {
            IXLCell headerCell = ws.DefinedName("SheetHeader").Ranges.First().FirstCell();
            headerCell.SetValue($"{group.Title}");
        }

        private static void RenderRows(IXLWorksheet ws, TableReportGroup group)
        {
            IXLRow formatRow = ws.Row(9);
            int currentRowIndex = 10;
            int id = 1;

            foreach (var row in group.Rows)
            {
                string lastName = row.GetValueOrDefault("fullName")?.ToString().Split(' ')[0] ?? "";
                string firstName = row.GetValueOrDefault("fullName")?.ToString().Split(' ')[1] ?? "";
                string gender = row.GetValueOrDefault("gender")?.ToString() ?? "";
                string formattedGender = gender switch
                {
                    "Male" => "ប",
                    "Female" => "ស",
                    _ => ""
                };
                string dob = row.GetValueOrDefault("dateOfBirth")?.ToString() ?? "";
                string birthPronvince = row.GetValueOrDefault("placeOfBirth")?.ToString() ?? "";
                string fatherName = row.GetValueOrDefault("fatherName")?.ToString() ?? "";
                string motherName = row.GetValueOrDefault("motherName")?.ToString() ?? "";
                string phone = row.GetValueOrDefault("phoneNumber")?.ToString() ?? "";
                string examCenter = row.GetValueOrDefault("examCenter")?.ToString() ?? "";
                string examDate = row.GetValueOrDefault("examDate")?.ToString() ?? "";
                string examRoom = row.GetValueOrDefault("examRoom")?.ToString() ?? "";
                string examTable = row.GetValueOrDefault("examTable")?.ToString() ?? "";

                IXLRow currentRow = ws.Row(currentRowIndex);
                formatRow.CopyTo(currentRow);

                currentRow.Cell("A").SetValue(id);
                currentRow.Cell("B").SetValue(lastName);
                currentRow.Cell("C").SetValue(firstName);
                currentRow.Cell("D").SetValue(formattedGender);
                currentRow.Cell("E").SetValue(dob);
                currentRow.Cell("F").SetValue(birthPronvince);
                currentRow.Cell("G").SetValue(fatherName);
                currentRow.Cell("H").SetValue(motherName);
                currentRow.Cell("I").SetValue(phone);
                currentRow.Cell("J").SetValue(examCenter);
                currentRow.Cell("K").SetValue(examDate);
                currentRow.Cell("L").SetValue(examRoom);
                currentRow.Cell("M").SetValue(examTable);

                currentRow.Unhide();

                if (id > group.Rows.Count)
                {
                    id = 1;
                    currentRowIndex = 10;
                    return;
                }
                
                if (!(id >= group.Rows.Count))
                    currentRow.InsertRowsBelow(1);

                currentRowIndex++;
                id++;
            }
        }

        private static void RenderFooter(IXLWorksheet ws, List<Dictionary<string, ReportCell>> rows, GroupedTableReportResult data)
        {
            // Summary section
            int total = rows.Count;
            int totalFemale = rows.Count(r => r.GetValueOrDefault("gender")?.Value?.ToString() == Gender.Female.ToString());

            string lastStudent = rows.LastOrDefault()?.GetValueOrDefault("fullName")?.Value?.ToString() ?? "";
            string summary = $"បញ្ឈប់បញ្ជីត្រឹមឈ្មោះ {lastStudent}    ចំនួនសរុប​ {total}  នាក់   ក្នុងនោះស្រី  {totalFemale}  នាក់ ។";

            IXLCell summaryCell = ws.DefinedName("SummaryRow").Ranges.First().FirstCell();

            summaryCell.SetValue(summary.UseKhmerNumbers());

            // Report date section
            IXLCell lunarDateCell = ws.DefinedName("ReportLunarDate").Ranges.First().FirstCell();
            IXLCell gregorianDateCell = ws.DefinedName("ReportGregorianDate").Ranges.First().FirstCell();

            DateTime reportDate = data.ReportDate ?? data.GeneratedDate;
            IKhmerLunarDate lunar = reportDate.ToKhmerLunarDate();
            int weekday = (int)reportDate.DayOfWeek;

            lunarDateCell.SetValue($"ថ្ងៃ{weekday.UseKhmerDays()} {lunar.LunarDay} ".UseKhmerNumbers() +
                $"ខែ {lunar.LunarMonth} ឆ្នាំ{lunar.ZodiacYear} {lunar.Stem} ព.ស.{lunar.LunarYear}".UseKhmerNumbers());

            gregorianDateCell.SetValue(($"វិ.ច.ប.ឯ.ដប.ប៉ប៉ ថ្ងៃទី {reportDate.Day} " +
                $"ខែ {reportDate.Month.UseKhmerMonths()} ឆ្នាំ{reportDate.Year}").UseKhmerNumbers());
        }
    }
}
