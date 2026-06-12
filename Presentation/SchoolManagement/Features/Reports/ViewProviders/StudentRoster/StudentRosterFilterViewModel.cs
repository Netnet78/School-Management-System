using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentRoster
{
    public partial class StudentRosterFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IAuthorizationService _authorizationService;
        private Timer? _searchDebounceTimer;

        public ReportTag ReportTypeKey => ReportTag.StudentRoster;

        public event Action? FilterChanged;

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

        public object GetFilterData()
        {
            return new StudentRosterFilter
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

        public async Task LoadAsync()
        {
            await Task.CompletedTask;
        }

        partial void OnIncludeInactiveChanged(bool value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnEnrollDateFromChanged(DateTime? value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnEnrollDateToChanged(DateTime? value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnReportDateChanged(DateTime value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnSearchKeywordChanged(string value)
        {
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(
                async (_) => { FilterChanged?.Invoke(); },
                null,
                400,
                Timeout.Infinite);
        }

        public void ResetFilterData()
        {
            IncludeInactive = false;
            SearchKeyword = string.Empty;
            EnrollDateFrom = null;
            EnrollDateTo = null;
            ReportDate = DateTime.Now;
        }
    }
}
