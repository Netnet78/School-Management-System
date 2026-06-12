using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Contracts
{
    public interface IPdfRenderer
    {
        bool CanRender(ReportResult result);

        void Render(IDocumentContainer document, ReportResult data);
    }
}

