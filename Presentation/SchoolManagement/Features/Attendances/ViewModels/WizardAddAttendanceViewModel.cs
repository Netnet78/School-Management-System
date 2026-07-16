using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public partial class WizardAddAttendanceViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private int _studentClassId;

        [ObservableProperty]
        private string _studentName = string.Empty;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private DateTime? _attendanceDate = DateTime.Today;

        [ObservableProperty]
        private string _scanTimeText = DateTime.Now.ToString("HH:mm");

        [ObservableProperty]
        private AttendanceStatus _attendanceStatus = AttendanceStatus.Present;

        [ObservableProperty]
        private string _otherInfo = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canManageAttendances;

        public IEnumerable<object> StatusOptions { get; } = Enum.GetValues<AttendanceStatus>()
            .Select(s => new { Value = s, Description = s.GetDescription() });

        public WizardAddAttendanceViewModel(
            IAttendanceService attendanceService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _attendanceService = attendanceService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is StudentSelectedParams p)
            {
                _studentClassId = p.StudentClassId;
                StudentName = p.StudentName;
                ClassName = p.ClassName;
            }

            return Task.CompletedTask;
        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            try
            {
                User? currentUser = _authorizationService.CurrentUser;
                if (currentUser == null)
                {
                    CanManageAttendances = false;
                    _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                ReturnResponse permission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageAttendances);
                if (permission.Status != Status.Success)
                {
                    CanManageAttendances = false;
                    _messageService.Show(permission.Message ?? "You do not have permission.", "Access denied", MessageButton.OK, MessageIcon.Hand);
                    return;
                }

                CanManageAttendances = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!CanManageAttendances)
            {
                return;
            }

            if (_studentClassId == 0)
            {
                _messageService.Show("Student class information is missing.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            if (!AttendanceDate.HasValue)
            {
                _messageService.Show("សូមមេត្តាជ្រើសរើសកាលបរិច្ឆេទ!", "ខ្វះព័ត៌មាន!", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(ScanTimeText) || !TimeOnly.TryParse(ScanTimeText, out _))
            {
                _messageService.Show("សូមមេត្តាបញ្ចូលម៉ោង (HH:mm)!", "ខ្វះព័ត៌មាន!", MessageButton.OK, MessageIcon.Information);
                return;
            }

            User? currentUser = _authorizationService.CurrentUser;
            if (currentUser == null)
            {
                _messageService.Show("សូមធ្វើការ login ចូលប្រព័ន្ធមួយនេះដើម្បីបន្តការប្រើប្រាស់.", "ឈប់សិន!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            IsLoading = true;

            try
            {
                Attendance attendance = new()
                {
                    StudentClassId = _studentClassId,
                    AttendanceDate = DateOnly.FromDateTime(AttendanceDate.Value),
                    ScanTime = TimeOnly.TryParse(ScanTimeText, out TimeOnly time)
                        ? time
                        : TimeOnly.FromDateTime(DateTime.Now),
                    Status = AttendanceStatus,
                    OtherInfo = OtherInfo.Trim(),
                    MarkedByEmployeeId = currentUser.EmployeeId
                };

                ReturnResponse response = await _attendanceService.InsertAsync(attendance);

                if (response.Status != Status.Success)
                {
                    _messageService.Show(response.Message ?? "Unable to save the attendance.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                _messageService.Show("Attendance added successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                await _navigationService.NavigateAsync<AttendanceViewModel>();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await _navigationService.NavigateAsync<AttendanceViewModel>();
        }
    }
}
