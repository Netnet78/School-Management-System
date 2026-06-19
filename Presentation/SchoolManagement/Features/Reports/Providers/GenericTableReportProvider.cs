using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Presentation.Features.Reports.Contracts;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Providers
{
    public class GenericTableReportProvider : TableReportProvider
    {
        private readonly IReportGenerator _generator;
        private readonly IReportFilterViewModel? _filterVm;

        public override string ReportTypeKey { get; }

        public override IReportFilterViewModel? FilterViewModel => _filterVm;

        public override string[] SupportedExportFormats => ["Excel"];

        public GenericTableReportProvider(
            string reportTypeKey,
            IReportGenerator generator,
            IReportFilterViewModel? filterVm,
            IReportRenderer renderer)
            : base(renderer)
        {
            ReportTypeKey = reportTypeKey;
            _generator = generator;
            _filterVm = filterVm;
        }

        protected override async Task<ReportResult> GenerateReportAsync(object filter, CancellationToken cancellationToken)
            => await _generator.GenerateAsync(filter, cancellationToken);
    }
}
