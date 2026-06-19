using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Descriptor
{
    public class ReportTypeDescriptor
    {
        public required string Key { get; init; }
        public required ReportDefinition Definition { get; init; }
        public required Type GeneratorType { get; init; }
        public required Type FilterVmType { get; init; }
        public string[] SupportedExportFormats { get; init; } = [];
        public Type? ProviderType { get; init; }
        
    }
}
