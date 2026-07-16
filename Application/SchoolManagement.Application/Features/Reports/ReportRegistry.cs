using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Descriptor;

namespace SchoolManagement.Application.Features.Reports
{
    public class ReportRegistry : IReportRegistry
    {
        private readonly Dictionary<string, ReportTypeDescriptor> _descriptors;

        public ReportRegistry(IEnumerable<ReportTypeDescriptor> descriptors)
        {
            _descriptors = descriptors
                .GroupBy(d => d.Key, StringComparer.Ordinal)
                .Select(g => g.Last())
                .ToDictionary(d => d.Key, StringComparer.Ordinal);
        }

        public ReportTypeDescriptor? GetDescriptor(string key) =>
            _descriptors.TryGetValue(key, out var desc) ? desc : null;

        public IReadOnlyList<ReportTypeDescriptor> GetAllDescriptors() => _descriptors.Values.ToList();
    }
}
