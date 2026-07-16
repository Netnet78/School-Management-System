using QuestPDF.Infrastructure;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Contracts
{
    public interface ICardPdfRenderer
    {
        bool CanRender(ReportResult result);
        void Render(IContainer container, CardDefinition cardGroup, CardRenderContext context);
        byte[] RenderToBytes(CardDefinition cardGroup, CardRenderContext context);
    }
}
