using ClosedXML.Excel;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class ExcelTemplateRenderer
    {
        public void FillCells(XLWorkbook workbook, ReportResult data)
        {
            var ws = workbook.Worksheet(1);

            foreach (var group in data.CardGroups ?? [])
            {
                foreach (var item in group.Items)
                {
                    if (item.Value == null)
                        continue;

                    int col = item.XPos + 1;
                    int row = item.YPos + 1;

                    var cell = ws.Cell(row, col);
                    SetCellValue(cell, item.Value);
                    ApplyStyle(cell, item);
                }
            }
        }

        private static void SetCellValue(IXLCell cell, object value)
        {
            cell.Value = value switch
            {
                string s => s,
                int i => i,
                long l => l,
                double d => d,
                float f => f,
                decimal m => Convert.ToDouble(m),
                bool b => b,
                byte[] => Blank.Value,
                null => Blank.Value,
                _ => value.ToString() ?? "",
            };
        }

        private static void ApplyStyle(IXLCell cell, ReportItem item)
        {
            if (item.FontSize > 0)
                cell.Style.Font.FontSize = item.FontSize;

            if (item.IsBold)
                cell.Style.Font.SetBold();

            if (!string.IsNullOrEmpty(item.FontColor))
            {
                try
                {
                    var color = item.FontColor.StartsWith('#')
                        ? XLColor.FromHtml(item.FontColor)
                        : XLColor.FromName(item.FontColor);
                    cell.Style.Font.FontColor = color;
                }
                catch
                {
                }
            }

            if (!string.IsNullOrEmpty(item.FontFamily))
            {
                cell.Style.Font.FontName = item.FontFamily;
            }
        }
    }
}
