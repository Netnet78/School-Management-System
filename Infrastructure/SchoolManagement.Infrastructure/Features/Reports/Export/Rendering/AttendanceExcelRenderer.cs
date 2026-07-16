using ClosedXML.Excel;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using KhmerCalendar;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class AttendanceExcelRenderer : IExcelRenderer
    {
        public bool CanRender(ReportResult result)
        {
            if (result.ReportTag != "attendance-report")
                return false;

            if (result is GroupedTableReportResult gr)
                return !string.IsNullOrWhiteSpace(gr.TemplatePath) && File.Exists(gr.TemplatePath);

            return false;
        }

        public XLWorkbook Render(ReportResult data)
        {
            var grouped = (GroupedTableReportResult)data;
            XLWorkbook workbook = new(grouped.TemplatePath);

            if (!workbook.TryGetWorksheet("Technical", out var defaultWs))
                throw new InvalidOperationException("Couldn't find 'Technical' in the attendance report template.");

            foreach (var group in grouped.Groups)
            {
                string sheetName = group.KhmerName ?? group.Name;
                if (string.IsNullOrWhiteSpace(sheetName)) sheetName = group.Name;
                if (sheetName.Length > 31) sheetName = sheetName[..31];

                var ws = defaultWs.CopyTo(sheetName);
                RenderSheet(ws, group.Columns, group.Rows, group.Title, grouped.GeneratedDate, grouped.ReportDate);
                RenderFooter(ws, data.ReportDate);
                ws.Cell("A1").Select();
            }

            if (workbook.Worksheets.Count > 1 && defaultWs != null)
                defaultWs.Delete();

            return workbook;
        }

        private static void RenderSheet(IXLWorksheet ws, List<ReportColumn> columns, List<Dictionary<string, ReportCell>> rows, string title, DateTime generatedDate, DateTime? reportDate)
        {
            // Template specifics
            int templateStartRow = 7;
            IXLRow formatRow = ws.Row(templateStartRow);

            // Find summary row (used to determine how many template rows exist)
            IXLCell summaryCell = ws.DefinedName("SummaryRow").Ranges.First().FirstCell();
            int summaryRowIndex = summaryCell.Address.RowNumber;
            int templateEndRow = summaryRowIndex - 1;

            // Determine day columns from provided columns (with header info for day-of-week)
            var dayColumnInfo = columns
                .Where(c => c.Key.StartsWith("day_"))
                .Select(c => new
                {
                    DayNumber = int.Parse(c.Key["day_".Length..]),
                    Header = c.HeaderKhmer ?? c.Header
                })
                .OrderBy(d => d.DayNumber)
                .ToList();

            var dayColumns = dayColumnInfo.Select(d => d.DayNumber).ToList();

            // Update title
            ws.Cell(2, 1).Value = title;

            // Update day headers: day-of-week in row 5, day number in row 6 (template starts at column D=4)
            for (int i = 0; i < dayColumnInfo.Count; i++)
            {
                int col = 4 + i;
                var info = dayColumnInfo[i];
                string rawHeader = info.Header;
                string dayOfWeek = rawHeader.Contains('\n') ? rawHeader.Split('\n')[0] : "";

                if (dayOfWeek.Equals("ព្រហស្បតិ៍")) dayOfWeek = "ព្រ-ហ";
                if (dayOfWeek.Equals("អាទិត្យ")) dayOfWeek = "អ-ទ";

                if (dayOfWeek.Equals("អ-ទ") || dayOfWeek.Equals("សៅរ៍"))
                {
                    ws.Cell(5, col).Style.Font.SetFontColor(XLColor.Red);
                    ws.Cell(6, col).Style.Font.SetFontColor(XLColor.Red);
                }

                ws.Cell(5, col).Value = dayOfWeek;
                ws.Cell(6, col).Value = info.DayNumber;
                ws.Column(col).Unhide();
            }

            // Set the black border for label info and labels
            ws.DefinedName("LabelInfo").Ranges.Cells().Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
            ws.DefinedName("Labels").Ranges.Cells().Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            // Hide remaining day columns
            for (int col = 4 + dayColumnInfo.Count; col <= 34; col++)
            {
                ws.Cell(5, col).Value = "";
                ws.Cell(6, col).Value = "";
                ws.Column(col).Hide();
            }

            // Pre-set thick borders on the format row so copied rows inherit them
            int lastDataCol = 3 + dayColumnInfo.Count;
            for (int c = 1; c <= lastDataCol; c++)
                formatRow.Cell(c).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            for (int c = lastDataCol + 1; c <= 34; c++)
                formatRow.Cell(c).Style.Border.OutsideBorder = XLBorderStyleValues.None;

            // Fill student data rows
            int currentRow = templateStartRow;

            for (int i = 0; i < rows.Count; i++)
            {
                var rowData = rows[i];

                if (currentRow > templateEndRow)
                {
                    ws.Row(templateEndRow).InsertRowsBelow(1);
                    templateEndRow++;
                    summaryRowIndex++;

                    IXLRow row = ws.Row(currentRow);
                    formatRow.CopyTo(row);
                }

                IXLRow newRow = ws.Row(currentRow);

                // Column A: sequential number
                var noValue = rowData.GetValueOrDefault("no")?.Value;
                newRow.Cell(1).Value = noValue != null ? Convert.ToDouble(noValue) : (i + 1);

                // Column B: student name
                newRow.Cell(2).Value = rowData.GetValueOrDefault("studentName")?.Value?.ToString() ?? "";

                // Column C: gender
                newRow.Cell(3).Value = rowData.GetValueOrDefault("gender")?.Value?.ToString() ?? "";

                // Daily columns
                for (int j = 0; j < dayColumns.Count; j++)
                {
                    int day = dayColumns[j];
                    int col = 4 + j;
                    if (col > 34) break;

                    newRow.Cell(col).Value = rowData[$"day_{day}"]?.ToString() ?? "";
                    bool hasRecord = !string.IsNullOrWhiteSpace((string?)rowData[$"day_{day}"].ToString());
                    if (!hasRecord)
                        newRow.Cell(col).Style.Fill.BackgroundColor = XLColor.Red;
                    else
                        newRow.Cell(col).Style.Fill.SetBackgroundColor(XLColor.White);
                }

                int labelCol = ws.DefinedName("Labels").Ranges.Cells().First().Address.ColumnNumber;
                newRow.Cell(labelCol).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

                int pCol = labelCol;
                int lCol = labelCol + 1;
                int aCol = labelCol + 2;
                int hCol = labelCol + 3;

                newRow.Cell(pCol).Value = (int)(rowData.GetValueOrDefault("excused")?.Value ?? 0);
                newRow.Cell(lCol).Value = (int)(rowData.GetValueOrDefault("late")?.Value ?? 0);
                newRow.Cell(aCol).Value = (int)(rowData.GetValueOrDefault("absent")?.Value ?? 0);
                newRow.Cell(hCol).Value = (int)(rowData.GetValueOrDefault("halfDay")?.Value ?? 0);

                newRow.Unhide();
                currentRow++;
            }

            // Apply thick borders to all data cells (visible columns only)
            int lastDataRow = currentRow - 1;
            for (int r = templateStartRow; r <= lastDataRow; r++)
            {
                for (int c = 1; c <= lastDataCol; c++)
                {
                    ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                }
                // Clear borders on hidden columns to prevent artifacts
                for (int c = lastDataCol + 1; c <= 34; c++)
                {
                    ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.None;
                }
            }

            // Hide remaining template rows
            for (int r = currentRow; r <= templateEndRow; r++)
                ws.Row(r).Hide();

            // Update summary/footer text
            var summaryData = rows.Count > 0 ? rows.LastOrDefault() : null;
            var totalStudents = rows.Count;
            var footerCell = ws.Cell(summaryRowIndex, 1);
            if (totalStudents > 0)
            {
                footerCell.Value = $"បញ្ឈប់បញ្ជីត្រឹម {rows.LastOrDefault()?.GetValueOrDefault("studentName")?.Value?.ToString() ?? ""}    ចំនួនសរុប {totalStudents} នាក់";
            }
        }


        private static void RenderFooter(IXLWorksheet ws, DateTime? reportDate)
        {
            try
            {
                var lunarCell = ws.DefinedName("ReportLunarDate").Ranges.First().FirstCell();
                var gregCell = ws.DefinedName("ReportGregorianDate").Ranges.First().FirstCell();

                DateTime actualReportDate = reportDate ?? DateTime.Now;
                IKhmerLunarDate lunar = actualReportDate.ToKhmerLunarDate();
                int weekday = (int)actualReportDate.DayOfWeek;

                lunarCell.SetValue($"ថ្ងៃ{weekday.UseKhmerDays()} {lunar.LunarDay} ".UseKhmerNumbers() +
                    $"ខែ {lunar.LunarMonth} ឆ្នាំ{lunar.ZodiacYear} {lunar.Stem} ព.ស.{lunar.LunarYear}".UseKhmerNumbers());

                gregCell.SetValue(("ដុនបូស្កូប៉ោយប៉ែត ថ្ងៃទី " + actualReportDate.Day + " " +
                    $"ខែ {actualReportDate.Month.UseKhmerMonths()} ឆ្នាំ{actualReportDate.Year}").UseKhmerNumbers());
            }
            catch
            {
                // ignore if template doesn't provide these named ranges
            }
        }
    }
}
