using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Infrastructure.Features.Reports.Export
{
    public class PdfExporter : IReportExporter
    {
        private readonly IEnumerable<IPdfRenderer> _pdfRenderers;
        private bool _licenseConfigured;

        public string FormatName => "PDF";

        public string FileExtension => ".pdf";

        public PdfExporter(IEnumerable<IPdfRenderer> pdfRenderers)
        {
            ConfigureQuestPdf();
            _pdfRenderers = pdfRenderers;
        }

        private void ConfigureQuestPdf()
        {
            if (_licenseConfigured)
            {
                return;
            }

            QuestPDF.Settings.License = LicenseType.Community;
            _licenseConfigured = true;
        }

        public async Task<byte[]> ExportAsync<TReportResult>(TReportResult data, CancellationToken cancellationToken = default)
            where TReportResult : ReportResult
        {
            var document = CreateDocument(data);
            var pdfBytes = document.GeneratePdf();
            return await Task.FromResult(pdfBytes);
        }

        public async Task ExportToFileAsync<TReportResult>(TReportResult data, string filePath, CancellationToken cancellationToken = default)
            where TReportResult : ReportResult
        {
            var document = CreateDocument(data);
            await Task.Run(() => document.GeneratePdf(filePath), cancellationToken);
            await Task.CompletedTask;
        }

        private Document CreateDocument(ReportResult data)
        {
            return Document.Create(container =>
            {
                var renderer = _pdfRenderers.FirstOrDefault(r => r.CanRender(data));
                if (renderer != null)
                {
                    renderer.Render(container, data);
                }
            });
        }
    }
}
