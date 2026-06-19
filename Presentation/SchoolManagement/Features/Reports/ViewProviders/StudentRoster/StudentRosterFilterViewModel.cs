using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentRoster
{
    public partial class StudentRosterFilterViewModel : ReportFilterViewModelBase<StudentRosterFilter>
    {
        private readonly IAuthorizationService _authorizationService;

        public override string ReportTypeKey => "student-roster";

        [ObservableProperty]
        private bool _includeInactive;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        [ObservableProperty]
        private int _startYear = DateTime.UtcNow.Year - 1;

        [ObservableProperty]
        private int _endYear = DateTime.UtcNow.Year;

        [ObservableProperty]
        private DateTime? _enrollDateFrom;

        [ObservableProperty]
        private DateTime? _enrollDateTo;

        [ObservableProperty]
        private DateTime _reportDate = DateTime.Now;

        public StudentRosterFilterViewModel(
            IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public override StudentRosterFilter GetFilterData()
        {
            return new()
            {
                IsActive = IncludeInactive ? null : true,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
                StartYear = StartYear,
                EndYear = EndYear,
                EnrollDateFrom = EnrollDateFrom.HasValue ? DateOnly.FromDateTime(EnrollDateFrom.Value) : null,
                EnrollDateTo = EnrollDateTo.HasValue ? DateOnly.FromDateTime(EnrollDateTo.Value) : null,
                ReportDate = DateOnly.FromDateTime(ReportDate),
            };
        }

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }

        partial void OnIncludeInactiveChanged(bool value) => OnFilterChanged();
        partial void OnEnrollDateFromChanged(DateTime? value) => OnFilterChanged();
        partial void OnEnrollDateToChanged(DateTime? value) => OnFilterChanged();
        partial void OnReportDateChanged(DateTime value) => OnFilterChanged();
        partial void OnSearchKeywordChanged(string value) => ScheduleDebouncedFilter();

        public override void ResetFilterData()
        {
            IncludeInactive = false;
            SearchKeyword = string.Empty;
            EnrollDateFrom = null;
            EnrollDateTo = null;
            ReportDate = DateTime.Now;
        }
    }
}
