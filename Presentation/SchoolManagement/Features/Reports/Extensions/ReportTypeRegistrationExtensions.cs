using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Descriptor;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Providers;
using SchoolManagement.Presentation.Features.Reports.Renderers;

namespace SchoolManagement.Presentation.Features.Reports.Extensions
{
    public static class ReportTypeRegistrationExtensions
    {
        private static bool _renderersRegistered;

        public static IServiceCollection AddReportType<TGenerator, TFilter>(
            this IServiceCollection services,
            Action<ReportTypeBuilder> configure)
            where TGenerator : class, IReportGenerator
            where TFilter : class, IReportFilterViewModel
        {
            var builder = new ReportTypeBuilder();
            configure(builder);
            var definition = builder.BuildDefinition();
            ReportTag key = builder.Key;

            services.AddTransient<IReportGenerator, TGenerator>();
            services.AddTransient<IReportFilterViewModel, TFilter>();

            EnsureRenderersRegistered(services);

            services.AddTransient<IReportViewProvider>(sp =>
            {
                var generators = sp.GetRequiredService<IEnumerable<IReportGenerator>>();
                var filterVms = sp.GetRequiredService<IEnumerable<IReportFilterViewModel>>();
                var generator = generators.First(g => g.ReportTypeKey == key);
                var filterVm = filterVms.FirstOrDefault(f => f.ReportTypeKey == key);

                if (builder.IsCardReport)
                {
                    return new StudentCardTableReportProvider(generator, filterVm);
                }

                if (builder.IsGroupedReport)
                {
                    return new GroupedTableViewProvider(key, generator, filterVm);
                }

                var renderers = sp.GetRequiredService<IEnumerable<IReportRenderer>>();
                return new GenericTableReportProvider(key, generator, filterVm, renderers.First(r => r is TableReportRenderer));
            });

            var descriptor = new ReportTypeDescriptor
            {
                Key = key,
                Definition = definition,
                GeneratorType = typeof(TGenerator),
                FilterVmType = typeof(TFilter),
            };

            services.AddSingleton(descriptor);

            return services;
        }

        private static void EnsureRenderersRegistered(IServiceCollection services)
        {
            if (_renderersRegistered)
                return;

            _renderersRegistered = true;
            services.AddSingleton<IReportRenderer, TableReportRenderer>();
        }
    }
}
