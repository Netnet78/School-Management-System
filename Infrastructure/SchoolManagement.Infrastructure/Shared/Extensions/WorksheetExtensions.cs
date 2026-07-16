using ClosedXML.Excel;

namespace SchoolManagement.Infrastructure.Shared.Extensions
{
    public static class WorksheetExtensions
    {
        /// <summary>
        /// Find a sheet with the name and if the sheet not found, create one.
        /// </summary>
        /// <param name="worksheets"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IXLWorksheet FirstOrAdd(this IXLWorksheets worksheets, string name)
        {
            if (worksheets.TryGetWorksheet(name, out var ws))
                return ws;

            return worksheets.Add(name);
        }
    }
}
