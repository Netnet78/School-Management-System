using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.Win32;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Models;
using System.Windows;

namespace New_Student_Management.Reports
{
    public class DepartmentReport
    {
        private const string BaseWorksheetName = "Computer";

        private readonly int _studyYearStart;
        private readonly int _studyYearEnd;
        private readonly int _startRow;
        private readonly IEnumerable<Candidate> _candidates;
        private readonly IPhotoFetchService _photoFetchService;
        public DepartmentReport(int? startYear, int? endYear, IEnumerable<Candidate> candidates, IPhotoFetchService photoFetchService)
        {
            _studyYearStart = startYear ?? (DateTime.Now.Month >= 7 ? DateTime.Now.Year : DateTime.Now.Year - 1);
            _studyYearEnd = endYear ?? (DateTime.Now.Month >= 7 ? DateTime.Now.Year + 1 : DateTime.Now.Year);
            _startRow = 4;
            _candidates = candidates;
            _photoFetchService = photoFetchService;
        }
        public string TemplatePath { get; set; } = @".\Sources\Spreadsheets\department_data.xlsm";
        public string OutputPath { get; private set; } = @"";
        public string FileName { get; private set; } = @"";

        public async Task<ReturnResponse> GenerateReport()
        {
            IEnumerable<IGrouping<int, Candidate>> skillGroups = _candidates
                .Where(c => c.Skill != null && !string.IsNullOrWhiteSpace(c.Skill.Name))
                .GroupBy(c => c.Skill.Id);

            XLWorkbook workbook = new(TemplatePath);

            foreach (var skillGroup in skillGroups)
            {
                var skill = skillGroup.First().Skill!;
                string sheetName = skill.Name;

                EnsureWorksheetExists(workbook, sheetName);
            }

            foreach (IGrouping<int, Candidate> studentsInSkill in skillGroups)
            {
                Candidate[] candidates = studentsInSkill.ToArray();

                if (candidates.Length == 0) continue;

                var skill = studentsInSkill.First().Skill!;

                string skillName = skill.Name;

                for (int i = 0; i < candidates.Length; i++)
                {
                    int id = i + 1;
                    await InsertStudent(
                        workbook,
                        candidates.ElementAt(i),
                        skillName,
                        id
                    );
                }
                UpdateSchoolYear(workbook, skillName);
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Save Generated Department Report",
                Filter = "Excel Workbook (*.xlsm)|*.xlsm",
                FileName = $"DepartmentReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsm",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                OutputPath = System.IO.Path.GetDirectoryName(saveFileDialog.FileName)!;
                FileName = System.IO.Path.GetFileName(saveFileDialog.FileName);
                try
                {
                    workbook.SaveAs(saveFileDialog.FileName);
                    return new() { Status= Status.Success };
                }
                catch (Exception ex)
                {
                    string message = $"Report failed to generate\n{ex.Message}";
                    return new() { Status = Status.Failed, Message = message };
                }
            }
            else
            {
                return new() { Status = Status.Rejected };
            }
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

        public async Task InsertStudent(XLWorkbook workbook, Candidate student, string sheetName, int index)
        {
            if (!workbook.TryGetWorksheet(sheetName, out var ws))
            {
                MessageBox.Show("Worksheet not found!", "Worksheet Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int startRow = _startRow;
            int endRow = startRow + 2;
            int startColumn = 1;
            int endColumn = 17;
            IXLRange range = ws.Range(startRow, startColumn, endRow, endColumn);

            int pasteDestination = endRow + (3 * (index - 1));
            range.CopyTo(ws.Cell(pasteDestination + 1, startColumn));

            int newStartRow = pasteDestination + 1;
            int newEndRow = newStartRow + 2;

            foreach (IXLRow row in ws.Rows(newStartRow, newEndRow))
            {
                row.Unhide();
                row.Height = 30;
            }

            // ID
            ws.Cell(newStartRow, 1).SetValue(index);
            // Full Name
            ws.Cell(newStartRow, 3).SetValue(student.LastName);
            ws.Cell(newStartRow, 4).SetValue(student.FirstName);
            // Latin Full Name
            ws.Cell(newStartRow + 1, 3).SetValue(student.LatinLastName);
            ws.Cell(newStartRow + 1, 4).SetValue(student.LatinFirstName);
            // Phone Number
            ws.Cell(newStartRow + 2, 3).SetValue(student.PhoneNumber);
            // Date of Birth
            ws.Cell(newStartRow + 2, 5).SetValue(student.DateOfBirth?.Day);
            ws.Cell(newStartRow + 1, 5).SetValue(student.DateOfBirth?.Month);
            ws.Cell(newStartRow, 5).SetValue(student.DateOfBirth?.Year);
            // Exam Degree
            ws.Cell(newStartRow, 6).SetValue("ឌីប្លូម (ថ្នាក់ទី៩)");
            // Place of Birth
            ws.Cell(newStartRow, 7).SetValue(student.BirthVillage);
            ws.Cell(newStartRow, 8).SetValue(student.BirthCommune);
            ws.Cell(newStartRow, 9).SetValue(student.BirthDistrict);
            ws.Cell(newStartRow, 10).SetValue(student.BirthProvince);
            // Father and Mother
            ws.Cell(newStartRow, 11).SetValue(student.FatherName);
            ws.Cell(newStartRow, 12).SetValue(student.FatherOccupation);
            ws.Cell(newStartRow, 13).SetValue(student.MotherName);
            ws.Cell(newStartRow, 14).SetValue(student.MotherOccupation);
            // Siblings
            if (student.SiblingsCount > 0)
            {
                ws.Cell(newStartRow, 15).SetValue(student.SiblingsCount);
            }
            else
            {
                ws.Cell(newStartRow, 15).SetValue("កូនទោល");
            }
            // Religion
            ws.Cell(newStartRow, 16).SetValue(student.Religion);
            // Stay type
            ws.Cell(newStartRow, 17).SetValue(student.StayType.GetDescription());

            FileObject? photo = (await _photoFetchService.GetStudentPhoto(student.PhotoKey)).Value;

            // Picture (1.25" x 0.93")
            if (photo != null)
            {
                int imageHeight = (int)Math.Floor(1.25 * 95.74f);
                int imageWidth = (int)Math.Round(0.94 * 95.74f);
                ws.AddPicture(photo.FilePath)
                  .MoveTo(
                      ws.Cell(newStartRow, 2),
                      1, // X offset
                      1  // Y offset
                  )
                  .WithSize(imageWidth, imageHeight)
                  .WithPlacement(XLPicturePlacement.Move);
            }
        }
        public void UpdateSchoolYear(XLWorkbook workbook, string sheetName)
        {
            if (!workbook.TryGetWorksheet(sheetName, out var ws))
            {
                return;
            }
            string title = $"DATA OF {sheetName.ToUpper()} DEPARTMENT STUDENTS  SCHOOL YEAR {_studyYearStart}-{_studyYearEnd + 2}";
            ws.Cell("R1").Value = title;
        }
    }

}
