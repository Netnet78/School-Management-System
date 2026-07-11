using SchoolManagement.Application.Features.Reports.Descriptor;

namespace SchoolManagement.Application.Features.Reports.Contracts
{
    public interface IReportRegistry
    {
        ReportTypeDescriptor? GetDescriptor(string key);

        IReadOnlyList<ReportTypeDescriptor> GetAllDescriptors();
    }
}
