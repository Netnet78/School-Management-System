using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.AuditLogs.ViewModels
{
    public partial class AuditLogViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuditLogService _auditLogService;
        private readonly IMessageService _messageService;

        public AuditLogViewModel(
            IAuthorizationService authorizationService,
            IAuditLogService auditLogService,
            IMessageService messageService)
        {
            _authorizationService = authorizationService;
            _auditLogService = auditLogService;
            _messageService = messageService;
        }

        [ObservableProperty]
        private bool _isLoading;
        [ObservableProperty]
        private DateTime? _fromDate;
        [ObservableProperty]
        private DateTime? _toDate;
        [ObservableProperty]
        private ObservableCollection<AuditLog> _auditLogs = [];
        [ObservableProperty]
        private BaseFilterObservableModel _filters = new();

        public async Task LoadLogs()
        {
            bool canProceed = await CanProceed();

            if (!canProceed)
            {
                _messageService.Show("???????????????? history ?????????????????????????????!",
                    "??????????????????????????????!",
                    MessageButton.OK,
                    MessageIcon.Information);
                return;
            }

            var logsResponse = await _auditLogService.GetAllAsync(
                Filters.CurrentPage, 
                Filters.PageSize,
                null,
                a => Filters.OrderBy == OrderType.Descending ? a.OrderByDescending(a => a.Id)
                : a.OrderBy(a => a.Id),
                a => a.User);

            if (logsResponse.Status == Status.Success && logsResponse.Value != null)
            {
                AuditLogs.Clear();
                
                foreach (AuditLog log in logsResponse.Value)
                {
                    AuditLogs.Add(log);
                }
            }
        }

        private async Task<bool> CanProceed()
        {
            ReturnResponse response = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageEmployees);
            return response.Status == Status.Success;
        }

        public async Task LoadAsync()
        {
            throw new NotImplementedException();
        }
    }
}

