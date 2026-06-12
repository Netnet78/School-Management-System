using ClosedXML.Excel;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Infrastructure.Features.Reports.Export
{
    public class ExcelExporter : IReportExporter
    {
        private readonly IEnumerable<IExcelRenderer> _renderers;

        public ExcelExporter(IEnumerable<IExcelRenderer> templateRenderers)
        {
            _renderers = templateRenderers;
        }

        public string FormatName => "Excel";

        public string FileExtension => ".xlsx";

        private async Task ExportToStreamAsync(ReportResult data, MemoryStream stream, CancellationToken cancellationToken)
        {
            using var workbook = CreateWorkbook(data);
            workbook.SaveAs(stream);
            await Task.CompletedTask;
        }

        private XLWorkbook CreateWorkbook(ReportResult data)
        {
            IExcelRenderer renderer = _renderers.FirstOrDefault(r => r.CanRender(data))
                ?? throw new InvalidOperationException("No excel renderer found for this report type.");

            return renderer.Render(data);
        }

        public async Task<byte[]> ExportAsync<TReportResult>(TReportResult data, CancellationToken cancellationToken = default)
            where TReportResult : ReportResult
        {
            using var stream = new MemoryStream();
            await ExportToStreamAsync(data, stream, cancellationToken);
            return stream.ToArray();
        }

        public async Task ExportToFileAsync<TReportResult>(
            TReportResult data,
            string filePath,
            CancellationToken cancellationToken = default)
            where TReportResult : ReportResult
        {
            using var workbook = CreateWorkbook(data);
            workbook.SaveAs(filePath);
            await Task.CompletedTask;
        }
    }
}
