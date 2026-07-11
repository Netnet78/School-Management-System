# Fix Attendance Report: Grouping & Formatting

## Problem Summary

1. **Grouping**: `AttendanceReportGenerator.cs` returns `TableReportResult` (flat), so all classes pour into one sheet
2. **Formatting**: `AttendanceExcelRenderer.cs` copies row 7's formatting over all template rows, destroying designer formatting

---

## File 1: `AttendanceReportGenerator.cs`

### Change: Return `GroupedTableReportResult`

**Location**: `Application/SchoolManagement.Application/Features/Reports/Generators/AttendanceReportGenerator.cs`

**What to do**:

1. Replace the flat row-building loop with a class-grouped approach
2. Group `studentClasses` by `ClassId`
3. For each class group, build rows with per-group sequential numbering (`seqNo` resets per group)
4. Create `TableReportGroup` per class with `Title` (appended class name), `Name`, `KhmerName`, `Columns`, `Rows`
5. Return `GroupedTableReportResult` with `Groups` and `CommonColumns`

**Exact code to replace** (lines 108-232):

Replace from `// Build rows` (line 108) through the end of `GenerateAsync` (line 232) with:

```csharp
            // Build dynamic day columns (shared across all groups)
            var columns = new List<ReportColumn>
            {
                new() { Key = "no", Header = "No.", HeaderKhmer = "ល.រ", DataType = typeof(int), Width = 50, Alignment = CellAlignment.Center },
                new() { Key = "studentName", Header = "Student Name", HeaderKhmer = "ឈ្មោះសិស្ស", Width = 200 },
                new() { Key = "gender", Header = "Gender", HeaderKhmer = "ភេទ", Width = 50, Alignment = CellAlignment.Center },
            };

            foreach (int day in dayNumbers)
            {
                columns.Add(new ReportColumn
                {
                    Key = $"day_{day}",
                    Header = day.ToString(),
                    HeaderKhmer = day.ToString(),
                    Width = 38,
                    Alignment = CellAlignment.Center,
                    FontSize = 11,
                });
            }

            columns.AddRange([
                new() { Key = "present", Header = "✓", HeaderKhmer = "វត្តមាន", DataType = typeof(int), Width = 60, Alignment = CellAlignment.Center },
                new() { Key = "late", Header = "L", HeaderKhmer = "យឺត", DataType = typeof(int), Width = 50, Alignment = CellAlignment.Center },
                new() { Key = "absent", Header = "A", HeaderKhmer = "អវត្តមាន", DataType = typeof(int), Width = 60, Alignment = CellAlignment.Center },
                new() { Key = "halfDay", Header = "H", HeaderKhmer = "ច្បាប់ចេញក្រៅ", DataType = typeof(int), Width = 50, Alignment = CellAlignment.Center },
                new() { Key = "excused", Header = "P", HeaderKhmer = "ច្បាប់", DataType = typeof(int), Width = 50, Alignment = CellAlignment.Center },
                new() { Key = "totalDays", Header = "Total", HeaderKhmer = "សរុប", DataType = typeof(int), Width = 60, Alignment = CellAlignment.Center },
                new() { Key = "attendanceRate", Header = "Rate (%)", HeaderKhmer = "អត្រា (%)", DataType = typeof(double), Format = "0.0", Width = 80, Alignment = CellAlignment.Center },
            ]);

            string baseTitle = "បញ្ជីស្រង់វត្តមានសិស្ស តាមជំនាញរបស់សាលាវិទ្យាល័យបច្ចេកទេស ដុនបូស្កូ ប៉ោយប៉ែត";

            if (attendanceFilter.Month != null)
                baseTitle = $"បញ្ជីស្រង់វត្តមានសិស្ស ប្រ​ចាំខែ {attendanceFilter.Month.Value.UseKhmerMonths()} " +
                $"ជំនាញ របស់សាលាវិទ្យាល័យបច្ចេកទេស ដុនបូស្កូ ប៉ោយប៉ែត";

            var classGroups = studentClasses.GroupBy(sc => sc.ClassId);

            var groups = new List<TableReportGroup>();
            int totalStudentCount = 0;

            foreach (var classGroup in classGroups)
            {
                var firstSc = classGroup.First();
                var @class = firstSc.Class;
                string className = @class?.KhmerName ?? @class?.Name ?? $"Class {classGroup.Key}";

                var groupRows = new List<Dictionary<string, ReportCell>>();
                int seqNo = 0;

                foreach (var sc in classGroup)
                {
                    seqNo++;
                    totalStudentCount++;
                    var student = sc.Student;
                    var candidate = student?.Candidate;
                    string genderCode = candidate?.Gender == Gender.Male ? "ប" : "ស";

                    var studentRecords = attendanceByStudent.GetValueOrDefault(sc.Id, []);

                    var dailyMap = studentRecords
                        .GroupBy(a => a.AttendanceDateTime.Day)
                        .ToDictionary(g => g.Key, g => g.OrderBy(a => a.AttendanceDateTime).Last().Status);

                    var row = new Dictionary<string, ReportCell>
                    {
                        ["no"] = seqNo,
                        ["studentName"] = candidate?.FullName ?? student?.FullName ?? "",
                        ["gender"] = genderCode,
                        ["latinName"] = candidate?.LatinFullName ?? "",
                        ["className"] = @class?.KhmerName ?? "",
                    };

                    foreach (int day in dayNumbers)
                    {
                        string statusCode = "";
                        if (dailyMap.TryGetValue(day, out var status))
                        {
                            statusCode = status switch
                            {
                                AttendanceStatus.Present => "✓",
                                AttendanceStatus.Late => "L",
                                AttendanceStatus.ExcusedLate => "L",
                                AttendanceStatus.Absent => "A",
                                AttendanceStatus.Excused => "P",
                                AttendanceStatus.EarlyLeave => "H",
                                _ => ""
                            };
                        }
                        row[$"day_{day}"] = statusCode;
                    }

                    int presentCount = studentRecords.Count(r => r.Status == AttendanceStatus.Present);
                    int lateCount = studentRecords.Count(r => r.Status == AttendanceStatus.Late || r.Status == AttendanceStatus.ExcusedLate);
                    int absentCount = studentRecords.Count(r => r.Status == AttendanceStatus.Absent);
                    int excusedCount = studentRecords.Count(r => r.Status == AttendanceStatus.Excused);
                    int halfDayCount = studentRecords.Count(r => r.Status == AttendanceStatus.EarlyLeave);
                    int totalCount = studentRecords.Count;

                    row["present"] = presentCount;
                    row["late"] = lateCount;
                    row["absent"] = absentCount;
                    row["excused"] = excusedCount;
                    row["halfDay"] = halfDayCount;
                    row["totalDays"] = totalCount;
                    row["attendanceRate"] = totalCount > 0
                        ? Math.Round((double)(presentCount + lateCount) / totalCount * 100, 1)
                        : 0.0;

                    groupRows.Add(row);
                }

                groups.Add(new TableReportGroup
                {
                    Title = $"{baseTitle} - {className}",
                    Name = @class?.Name ?? $"Class {classGroup.Key}",
                    KhmerName = @class?.KhmerName,
                    Columns = columns,
                    Rows = groupRows,
                });
            }

            return new GroupedTableReportResult
            {
                ReportTag = "attendance-report",
                Title = baseTitle,
                SubTitle = $"Attendance Report - {attendanceFilter.ClassId?.ToString() ?? "All Classes"}",
                GeneratedDate = DateTime.UtcNow,
                TemplatePath = _templatePath,
                Groups = groups,
                CommonColumns = columns,
                Summary = new Dictionary<string, object>
                {
                    ["__totalCount"] = totalStudentCount,
                    ["សិស្សសរុប"] = totalStudentCount,
                    ["ភាគរយនៃវត្តមានជាមធ្យម"] = totalStudentCount > 0
                        ? Math.Round(groups.Average(g => g.Rows.Count > 0
                            ? g.Rows.Average(r => Convert.ToDouble(r.GetValueOrDefault("attendanceRate")?.Value ?? 0d))
                            : 0d), 1)
                        : 0.0,
                }
            };
```

---

## File 2: `AttendanceExcelRenderer.cs`

### Change 2a: Restrict `CanRender` to grouped data only

**Location**: `Infrastructure/SchoolManagement.Infrastructure/Features/Reports/Export/Rendering/AttendanceExcelRenderer.cs`, line 10-23

Replace:
```csharp
        public bool CanRender(ReportResult result)
        {
            // Support both single table and grouped table attendance report results
            if (result.ReportTag != "attendance-report")
                return false;

            string? templatePath = null;
            if (result is TableReportResult tr)
                templatePath = tr.TemplatePath;
            else if (result is GroupedTableReportResult gr)
                templatePath = gr.TemplatePath;

            return !string.IsNullOrWhiteSpace(templatePath) && File.Exists(templatePath);
        }
```

With:
```csharp
        public bool CanRender(ReportResult result)
        {
            if (result.ReportTag != "attendance-report")
                return false;

            if (result is GroupedTableReportResult gr)
                return !string.IsNullOrWhiteSpace(gr.TemplatePath) && File.Exists(gr.TemplatePath);

            return false;
        }
```

### Change 2b: Remove `TableReportResult` branch from `Render`

**Location**: lines 25-66

Replace:
```csharp
        public XLWorkbook Render(ReportResult data)
        {
            // Support both TableReportResult and GroupedTableReportResult
            XLWorkbook workbook;

            if (data is TableReportResult tableData)
            {
                workbook = new XLWorkbook(tableData.TemplatePath);

                if (!workbook.TryGetWorksheet("Technical", out var ws))
                    throw new InvalidOperationException("Couldn't find 'Technical' in the attendance report template.");

                RenderSheet(ws, tableData.Columns, tableData.Rows, tableData.Title, tableData.GeneratedDate, tableData.ReportDate);
            }
            else if (data is GroupedTableReportResult grouped)
            {
                workbook = new XLWorkbook(grouped.TemplatePath);

                if (!workbook.TryGetWorksheet("Technical", out var defaultWs))
                    throw new InvalidOperationException("Couldn't find 'Technical' in the attendance report template.");

                foreach (var group in grouped.Groups)
                {
                    string sheetName = group.KhmerName ?? group.Name;
                    if (string.IsNullOrWhiteSpace(sheetName)) sheetName = group.Name;
                    if (sheetName.Length > 31) sheetName = sheetName[..31];

                    var ws = defaultWs.CopyTo(sheetName);
                    RenderSheet(ws, group.Columns, group.Rows, group.Title, grouped.GeneratedDate, grouped.ReportDate);
                    ws.Cell("A1").Select();
                }

                if (workbook.Worksheets.Count > 1 && defaultWs != null)
                    defaultWs.Delete();
            }
            else
            {
                throw new InvalidOperationException("Unsupported report result for attendance renderer.");
            }

            return workbook;
        }
```

With:
```csharp
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
                ws.Cell("A1").Select();
            }

            if (workbook.Worksheets.Count > 1 && defaultWs != null)
                defaultWs.Delete();

            return workbook;
        }
```

### Change 2c: Fix `RenderSheet` format copying and hide/unhide

**Problem**: `formatRow.CopyTo(newRow)` runs on every iteration, overwriting pre-existing template rows' designer-set formatting. Should only copy format when inserting new rows beyond the template range.

**Location**: lines 68-181

Replace the entire `RenderSheet` method:

```csharp
        private void RenderSheet(IXLWorksheet ws, List<ReportColumn> columns, List<Dictionary<string, ReportCell>> rows, string title, DateTime generatedDate, DateTime? reportDate)
        {
            int templateStartRow = 7;
            IXLRow formatRow = ws.Row(templateStartRow);

            IXLCell summaryCell = ws.DefinedName("SummaryRow").Ranges.First().FirstCell();
            int summaryRowIndex = summaryCell.Address.RowNumber;
            int templateEndRow = summaryRowIndex - 1;

            var dayColumns = columns
                .Where(c => c.Key.StartsWith("day_"))
                .Select(c => int.Parse(c.Key["day_".Length..]))
                .OrderBy(d => d)
                .ToList();

            ws.Cell(2, 1).Value = title;

            for (int i = 0; i < dayColumns.Count; i++)
            {
                int day = dayColumns[i];
                int col = 4 + i;
                ws.Cell(6, col).Value = day;
                ws.Column(col).Unhide();
            }

            for (int col = 4 + dayColumns.Count; col <= 34; col++)
            {
                ws.Cell(6, col).Value = "";
                ws.Column(col).Hide();
            }

            int currentRow = templateStartRow;

            for (int i = 0; i < rows.Count; i++)
            {
                var rowData = rows[i];

                if (currentRow > templateEndRow)
                {
                    ws.Row(templateEndRow).InsertRowsBelow(1);
                    templateEndRow++;
                    summaryRowIndex++;

                    IXLRow newRow = ws.Row(currentRow);
                    formatRow.CopyTo(newRow);
                }

                IXLRow newRow = ws.Row(currentRow);

                var noValue = rowData.GetValueOrDefault("no")?.Value;
                newRow.Cell(1).Value = noValue != null ? Convert.ToDouble(noValue) : (i + 1);

                newRow.Cell(2).Value = rowData.GetValueOrDefault("studentName")?.Value?.ToString() ?? "";

                newRow.Cell(3).Value = rowData.GetValueOrDefault("gender")?.Value?.ToString() ?? "";

                for (int j = 0; j < dayColumns.Count; j++)
                {
                    int day = dayColumns[j];
                    int col = 4 + j;
                    if (col > 34) break;

                    string? statusCode = rowData.GetValueOrDefault($"day_{day}")?.Value?.ToString();
                    newRow.Cell(col).Value = statusCode ?? string.Empty;
                }

                newRow.Unhide();
                currentRow++;
            }

            for (int r = currentRow; r <= templateEndRow; r++)
                ws.Row(r).Hide();

            var totalStudents = rows.Count;
            var footerCell = ws.Cell(summaryRowIndex, 1);
            if (totalStudents > 0)
            {
                footerCell.Value = $"បញ្ឈប់បញ្ជីត្រឹម {rows.LastOrDefault()?.GetValueOrDefault("studentName")?.Value?.ToString() ?? ""}    ចំនួនសរុប {totalStudents} នាក់";
            }

            try
            {
                var lunarCell = ws.DefinedName("ReportLunarDate").Ranges.First().FirstCell();
                var gregCell = ws.DefinedName("ReportGregorianDate").Ranges.First().FirstCell();

                DateTime actualReportDate = reportDate ?? generatedDate;
                IKhmerLunarDate lunar = actualReportDate.ToKhmerLunarDate();
                int weekday = (int)actualReportDate.DayOfWeek;

                lunarCell.SetValue($"ថ្ងៃ{weekday.UseKhmerDays()} {lunar.LunarDay} ".UseKhmerNumbers() +
                    $"ខែ {lunar.LunarMonth} ឆ្នាំ{lunar.ZodiacYear} {lunar.Stem} ព.ស.{lunar.LunarYear}".UseKhmerNumbers());

                gregCell.SetValue(("ដុនបូស្កូប៉ោយប៉ែត ថ្ងៃទី " + actualReportDate.Day + " " +
                    $"ខែ {actualReportDate.Month.UseKhmerMonths()} ឆ្នាំ{actualReportDate.Year}").UseKhmerNumbers());
            }
            catch
            {
            }
        }
```

**Key change**: `formatRow.CopyTo(newRow)` now only runs inside the `if (currentRow > templateEndRow)` block — so pre-existing template rows (7, 8, 9, ...) keep their original designer formatting. Only newly inserted rows get format copied from row 7.

---

## Verification

- Build the solution to check for compilation errors
- Run any existing report-related tests
- Test with:
  - Single class filter → should produce one sheet named after the class
  - All classes (no filter) → should produce one sheet per class
  - Verify font/border formatting matches the template design
