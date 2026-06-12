using SchoolManagement.Application.Features.Reports.Descriptor;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Contracts
{
    public interface IReportRegistry
    {
        ReportTypeDescriptor? GetDescriptor(ReportTag key);

        IReadOnlyList<ReportTypeDescriptor> GetAllDescriptors();
    }
}
