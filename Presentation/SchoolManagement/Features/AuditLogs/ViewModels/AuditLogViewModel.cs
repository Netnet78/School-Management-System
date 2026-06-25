using System.Collections.ObjectModel;
using System.Linq.Expressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.AuditLogs.ViewModels
{
    public partial class AuditLogViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuditLogService _auditLogService;
        private readonly IMessageService _messageService;

        private const int DefaultPageSize = 10;

        private CancellationTokenSource? _cts;

        [ObservableProperty]
        private ObservableCollection<AuditLog> _auditLogs = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canViewAuditLogs;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime? _fromDate;

        [ObservableProperty]
        private DateTime? _toDate;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _currentPageTotalCount;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _hasPreviousPage;

        public AuditLogViewModel(
            IAuthorizationService authorizationService,
            IAuditLogService auditLogService,
            IMessageService messageService)
        {
            _authorizationService = authorizationService;
            _auditLogService = auditLogService;
            _messageService = messageService;
        }

        public async Task LoadAsync()
        {
            if (!_authorizationService.UserIsAdmin)
            {
                CanViewAuditLogs = false;
                _messageService.Show("អ្នកមិនអាចមើលប្រវត្តិប្រតិបត្តិការបានទេ!", "គ្មានការអនុញ្ញាត!", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            CanViewAuditLogs = true;
            await LoadLogsAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadLogsAsync();
        }

        [RelayCommand]
        private async Task ResetFiltersAsync()
        {
            SearchText = string.Empty;
            FromDate = null;
            ToDate = null;
            CurrentPage = 1;
            await LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            if (!CanViewAuditLogs)
            {
                return;
            }

            IsLoading = true;

            try
            {
                IEnumerable<FilterCondition<AuditLog>> filters = BuildFilter();
                Expression<Func<AuditLog, bool>>? predicate = BuildSearchPredicate();

                var response = await _auditLogService.GetAllAsync(
                    page: CurrentPage,
                    pageSize: DefaultPageSize + 1,
                    filters: filters,
                    extraPredicate: predicate,
                    orderBy: [new SortCriteria<AuditLog>("Timestamp", OrderDirection.Descending)],
                    includes: ["User"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    List<AuditLog> logs = response.Value.ToList();
                    HasNextPage = logs.Count > DefaultPageSize;

                    AuditLogs.Clear();
                    foreach (AuditLog log in logs.Take(DefaultPageSize))
                    {
                        AuditLogs.Add(log);
                    }
                }
                else
                {
                    AuditLogs.Clear();
                    HasNextPage = false;
                }

                HasPreviousPage = CurrentPage > 1;
                CurrentPageTotalCount = AuditLogs.Count;
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading audit logs: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private List<FilterCondition<AuditLog>> BuildFilter()
        {
            List<FilterCondition<AuditLog>> filters = [];

            if (FromDate.HasValue)
            {
                filters.Add(new(a => a.Timestamp, FilterOperator.GreaterThanOrEqual, FromDate.Value));
            }

            if (ToDate.HasValue)
            {
                filters.Add(new(a => a.Timestamp, FilterOperator.LessThanOrEqual, ToDate.Value));
            }

            return filters;
        }

        private Expression<Func<AuditLog, bool>>? BuildSearchPredicate()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return null;

            string search = SearchText.Trim();
            return a =>
                (a.EntityType != null && a.EntityType.Contains(search)) ||
                (a.Action != null && a.Action.Contains(search)) ||
                (a.User != null && a.User.Username != null && a.User.Username.Contains(search)) ||
                (a.EntityName != null && a.EntityName.Contains(search));
        }

        async partial void OnSearchTextChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                await Task.Delay(300, token);
                CurrentPage = 1;
                await LoadLogsAsync();
            }
            catch (TaskCanceledException) { }
        }

        async partial void OnFromDateChanged(DateTime? value)
        {
            CurrentPage = 1;
            await LoadLogsAsync();
        }

        async partial void OnToDateChanged(DateTime? value)
        {
            CurrentPage = 1;
            await LoadLogsAsync();
        }

        async partial void OnCurrentPageChanged(int oldValue, int newValue)
        {
            if (IsLoading) return;

            if (newValue < 1)
            {
                CurrentPage = 1;
                return;
            }

            await LoadLogsAsync();
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (HasNextPage)
            {
                CurrentPage++;
            }
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
            }
        }
    }
}
