using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Descriptor;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports
{
    public class ReportRegistry : IReportRegistry
    {
        private readonly Dictionary<ReportTag, ReportTypeDescriptor> _descriptors;

        public ReportRegistry(IEnumerable<ReportTypeDescriptor> descriptors)
        {
            _descriptors = descriptors.ToDictionary(d => d.Key);
        }

        public ReportTypeDescriptor? GetDescriptor(ReportTag key) =>
            _descriptors.TryGetValue(key, out var desc) ? desc : null;

        public IReadOnlyList<ReportTypeDescriptor> GetAllDescriptors() => _descriptors.Values.ToList();
    }
}
