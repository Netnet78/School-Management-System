using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.Providers
{
    public class GenericTableReportProvider : TableReportViewProvider
    {
        private readonly IReportGenerator _generator;
        private readonly IReportFilterViewModel? _filterVm;

        public override ReportTag ReportTypeKey { get; }

        public override IReportFilterViewModel? FilterViewModel => _filterVm;

        public GenericTableReportProvider(
            ReportTag reportTypeKey,
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
