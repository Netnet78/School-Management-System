using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Descriptor;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Attributes;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Presentation.Features.Reports.Providers;
using SchoolManagement.Presentation.Features.Reports.Renderers;

namespace SchoolManagement.Presentation.Features.Reports.Extensions
{
    public static class ReportTypeRegistrationExtensions
    {
        public static IServiceCollection AddReportTypesFromAssembly(
            this IServiceCollection services,
            Assembly generatorAssembly)
        {
            Assembly presentationAssembly = typeof(ReportTypeRegistrationExtensions).Assembly;

            List<Type> reportTypes = generatorAssembly.GetTypes()
                .Where(t => t.GetCustomAttribute<ReportTypeAttribute>() != null)
                .Where(t => typeof(IReportGenerator).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ToList();

            services.AddSingleton<IReportRenderer, TableReportRenderer>();

            var filterVmLookup = presentationAssembly.GetTypes()
                .Where(t => typeof(IReportFilterViewModel).IsAssignableFrom(t)
                         && !t.IsAbstract && !t.IsInterface
                         && t.Name.EndsWith("FilterViewModel"))
                .ToDictionary(t => t.Name[..^"FilterViewModel".Length],
                              t => t,
                              StringComparer.OrdinalIgnoreCase);

            var optionsVmLookup = presentationAssembly.GetTypes()
                .Where(t => typeof(IReportOptionsViewModel).IsAssignableFrom(t)
                         && !t.IsAbstract && !t.IsInterface
                         && t.Name.EndsWith("OptionsViewModel"))
                .ToDictionary(t => t.Name[..^"OptionsViewModel".Length],
                              t => t,
                              StringComparer.OrdinalIgnoreCase);

            foreach (Type generatorType in reportTypes)
            {
                ReportTypeAttribute attr = generatorType.GetCustomAttribute<ReportTypeAttribute>()!;
                string reportTag = attr.Key;

                services.AddTransient(generatorType, generatorType);
                services.AddTransient(typeof(IReportGenerator), generatorType);

                Type? filterVmType = FindFilterViewModelType(presentationAssembly, generatorType, filterVmLookup);
                if (filterVmType != null)
                {
                    services.AddTransient(filterVmType, filterVmType);
                    services.AddTransient(typeof(IReportFilterViewModel), filterVmType);
                }

                Type? optionsVmType = FindOptionsViewModelType(presentationAssembly, generatorType, optionsVmLookup);
                if (optionsVmType != null)
                {
                    services.AddTransient(optionsVmType, optionsVmType);
                    services.AddTransient(typeof(IReportOptionsViewModel), optionsVmType);
                }

                Type? capturedFilterVmType = filterVmType;
                Type? capturedOptionsVmType = optionsVmType;

                services.AddTransient<IReportViewProvider>(sp =>
                {
                    IReportGenerator generator = (IReportGenerator)sp.GetRequiredService(generatorType);
                    IReportFilterViewModel? filterVm = (IReportFilterViewModel?)(capturedFilterVmType != null
                        ? sp.GetRequiredService(capturedFilterVmType)
                        : null);
                    IReportOptionsViewModel? optionsVm = (IReportOptionsViewModel?)(capturedOptionsVmType != null
                        ? sp.GetRequiredService(capturedOptionsVmType)
                        : null);

                    if (attr.ReportStyle is ReportStyle.Card or ReportStyle.GroupedCard)
                    {
                        return new CardTableReportProvider(generator.ReportTypeKey, generator, filterVm, optionsVm);
                    }

                    if (attr.ReportStyle == ReportStyle.GroupedTable)
                    {
                        return new GroupedTableReportProvider(generator.ReportTypeKey, generator, filterVm);
                    }

                    var renderers = sp.GetRequiredService<IEnumerable<IReportRenderer>>();
                    return new GenericTableReportProvider(generator.ReportTypeKey, generator, filterVm,
                        renderers.First(r => r is TableReportRenderer));
                });

                ReportDefinition definition = new()
                {
                    Key = reportTag,
                    DisplayName = attr.DisplayName,
                    DisplayNameKhmer = attr.DisplayNameKhmer,
                    Description = attr.Description,
                    IconKind = attr.IconKind,
                    SortOrder = attr.SortOrder,
                };

                ReportTypeDescriptor descriptor = new()
                {
                    Key = reportTag,
                    Definition = definition,
                    GeneratorType = generatorType,
                    FilterVmType = filterVmType ?? generatorType,
                    SupportedExportFormats = attr.SupportedExportFormats ?? [],
                };
                services.AddSingleton(descriptor);
            }

            return services;
        }

        private static Type? FindFilterViewModelType(
            Assembly presentationAssembly,
            Type generatorType,
            Dictionary<string, Type> filterVmLookup)
        {
            var generatorName = generatorType.Name;
            if (!generatorName.EndsWith("Generator"))
                return null;

            var baseName = generatorName[..^"Generator".Length];

            var exactType = presentationAssembly.GetType(
                $"SchoolManagement.Presentation.Features.Reports.ViewProviders.{baseName}.{baseName}FilterViewModel");
            if (exactType != null)
                return exactType;

            if (filterVmLookup.TryGetValue(baseName, out Type? match))
                return match;

            if (baseName.EndsWith("Report", StringComparison.OrdinalIgnoreCase))
            {
                string withoutReport = baseName[..^"Report".Length];
                if (filterVmLookup.TryGetValue(withoutReport, out match))
                    return match;
            }

            return null;
        }

        private static Type? FindOptionsViewModelType(
            Assembly presentationAssembly,
            Type generatorType,
            Dictionary<string, Type> optionsVmLookup)
        {
            var generatorName = generatorType.Name;
            if (!generatorName.EndsWith("Generator"))
                return null;

            var baseName = generatorName[..^"Generator".Length];

            var exactType = presentationAssembly.GetType(
                $"SchoolManagement.Presentation.Features.Reports.ViewProviders.{baseName}.{baseName}OptionsViewModel");
            if (exactType != null)
                return exactType;

            if (optionsVmLookup.TryGetValue(baseName, out Type? match))
                return match;

            return null;
        }
    }
}
