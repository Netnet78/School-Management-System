using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Services
{
    public class ReportComponentFactory : IReportComponentFactory
    {
        private readonly IEnumerable<IReportGenerator> _generators;
        private readonly IServiceProvider _serviceProvider;

        public ReportComponentFactory(
            IEnumerable<IReportGenerator> generators,
            IServiceProvider serviceProvider)
        {
            _generators = generators;
            _serviceProvider = serviceProvider;
        }

        public IReportGenerator CreateGenerator(ReportDefinition definition)
        {
            return _generators.First(g => g.ReportTypeKey == definition.Key);
        }

        public IReportFilterViewModel CreateFilterViewModel(ReportDefinition definition)
        {
            return (IReportFilterViewModel)_serviceProvider.GetRequiredService(definition.FilterViewModelType);
        }
    }
}
