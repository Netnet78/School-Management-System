using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Contracts
{
    public interface ICardRenderer
    {
        void RenderCard(IContainer container, ReportItemGroup cardGroup);
    }
}
