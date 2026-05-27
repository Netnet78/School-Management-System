using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports
{
    public class ReportRegistry : IReportRegistry
    {
        private readonly Dictionary<string, ReportDefinition> _definitions;

        public ReportRegistry(IEnumerable<ReportDefinition> definitions)
        {
            _definitions = definitions
                .OrderBy(d => d.SortOrder)
                .ToDictionary(d => d.Key);
        }

        public IReadOnlyList<ReportDefinition> GetAll() => _definitions.Values.ToList();

        public ReportDefinition? GetByKey(string key) =>
            _definitions.TryGetValue(key, out var def) ? def : null;
    }
}
