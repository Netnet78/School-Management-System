using SchoolManagement.Application.Features.Reports.Descriptor;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Contracts
{
    public interface IReportRegistry
    {
        ReportTypeDescriptor? GetDescriptor(string key);

        IReadOnlyList<ReportTypeDescriptor> GetAllDescriptors();
    }
}
