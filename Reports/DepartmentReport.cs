using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.Win32;
using New_Student_Management.Helpers;
using New_Student_Management.Models;
using System.Windows;

namespace New_Student_Management.Reports
{
    public class DepartmentReport
    {
        private readonly List<Student> _students;
        private readonly int _studyYearStart;
        private readonly int _studyYearEnd;
        private readonly int _startRow;
        public DepartmentReport(List<Student> students, int? startYear, int? endYear)
        {
            _students = students;
            _studyYearStart = startYear ?? (DateTime.Now.Month >= 9 ? DateTime.Now.Year : DateTime.Now.Year - 1);
            _studyYearEnd = endYear ?? (DateTime.Now.Month >= 9 ? DateTime.Now.Year + 1 : DateTime.Now.Year);
            _startRow = 4;
        }
        public string TemplatePath { get; set; } = @".\Sources\Spreadsheets\department_data.xlsm";
        public string OutputPath { get; private set; } = @"";
        public string FileName { get; private set; } = @"";

        public ReturnStatus GenerateReport()
        {
            XLWorkbook workbook = new(TemplatePath);
            Dictionary<StudentSkill, List<Student>> studentSkillPairs = new()
            {
                { StudentSkill.Computer, _students.Where(s => s.Skill == StudentSkill.Computer).ToList() },
                { StudentSkill.Electrical, _students.Where(s => s.Skill == StudentSkill.Electrical).ToList() },
                { StudentSkill.CNC, _students.Where(s => s.Skill == StudentSkill.CNC).ToList() },
            };

            foreach (KeyValuePair<StudentSkill, List<Student>> studentsInSkill in studentSkillPairs)
            {
                if (studentsInSkill.Value.Count <= 0) continue;

                string skillGroup = studentsInSkill.Key.ToString();
                List<Student> students = studentsInSkill.Value;

                for (int i = 0; i < students.Count; i++)
                {
                    int id = i + 1;
                    InsertStudent(
                        workbook,
                        students[i],
                        skillGroup,
                        id
                    );
                }
                UpdateSchoolYear(workbook, skillGroup);
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

        public void InsertStudent(XLWorkbook workbook, Student student, string sheetName, int index)
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
            ws.Cell(newStartRow + 2, 5).SetValue(student.DateOfBirth.Day);
            ws.Cell(newStartRow + 1, 5).SetValue(student.DateOfBirth.Month);
            ws.Cell(newStartRow, 5).SetValue(student.DateOfBirth.Year);
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

            // Picture (1.25" x 0.93")
            if (!string.IsNullOrWhiteSpace(student.PhotoPath))
            {
                int imageHeight = (int)Math.Floor(1.25 * 95.74f);
                int imageWidth = (int)Math.Round(0.94 * 95.74f);
                ws.AddPicture(student.PhotoPath)
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
                MessageBox.Show("Worksheet not found!", "Worksheet Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string title = $"DATA OF {sheetName.ToUpper()} DEPARTMENT STUDENTS  SCHOOL YEAR {_studyYearStart}-{_studyYearEnd}";
            ws.Cell("R1").Value = title;
        }
    }

}
