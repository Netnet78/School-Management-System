using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Shared.Enums;
using SchoolManagement.Core.Shared.Extensions;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public abstract partial class AttendanceFormViewModelBase : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IStudentClassService _studentClassService;
        private readonly IAuthorizationService _authorizationService;
        protected readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        [ObservableProperty]
        private ObservableCollection<object> _studentClassOptions = [];

        [ObservableProperty]
        private int? _selectedStudentClassId;

        [ObservableProperty]
        private bool _canManageAttendances;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private DateTime? _attendanceDate = DateTime.Today;

        [ObservableProperty]
        private string _scanTimeText = DateTime.Now.ToString("HH:mm:ss");

        [ObservableProperty]
        private AttendanceStatus _attendanceStatus = AttendanceStatus.Present;

        [ObservableProperty]
        private string _otherInfo = string.Empty;

        public IEnumerable<object> StatusOptions { get; } = Enum.GetValues<AttendanceStatus>()
            .Select(s => new { Value = s, Description = s.GetDescription() });

        protected AttendanceFormViewModelBase(
            IStudentClassService studentClassService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _studentClassService = studentClassService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public abstract Task LoadAsync();

        protected async Task LoadCommonAsync()
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
                _messageService.Show(permission.Message ?? "You do not have permission to manage attendances.", "Access denied", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            CanManageAttendances = true;

            await LoadStudentClassOptionsAsync();
        }

        private async Task LoadStudentClassOptionsAsync()
        {
            try
            {
                ReturnResponse<IEnumerable<StudentClass>> response = await _studentClassService.GetAllAsync(
                    filters: [new FilterCondition<StudentClass>(sc => sc.IsActive, FilterOperator.Equals, true)],
                    page: 1,
                    pageSize: int.MaxValue,
                    orderBy: [new(s => s.Student.Candidate.FullName)],
                    includes: ["Student", "Student.Candidate", "Class"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    StudentClassOptions.Clear();
                    foreach (StudentClass sc in response.Value)
                    {
                        StudentClassOptions.Add(new
                        {
                            Value = (int?)sc.Id,
                            Description = $"{sc.Student.FullName} - {sc.Class.KhmerName}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading student classes: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
        }

        protected void ResetForm()
        {
            SelectedStudentClassId = null;
            AttendanceDate = DateTime.Today;
            ScanTimeText = DateTime.Now.ToString("HH:mm");
            AttendanceStatus = AttendanceStatus.Present;
            OtherInfo = string.Empty;
        }

        protected Attendance BuildAttendanceFromForm()
        {
            User? currentUser = _authorizationService.CurrentUser;

            return new Attendance
            {
                StudentClassId = SelectedStudentClassId ?? 0,
                AttendanceDate = AttendanceDate.HasValue
                    ? DateOnly.FromDateTime(AttendanceDate.Value)
                    : DateOnly.FromDateTime(DateTime.Today),
                ScanTime = TimeOnly.TryParse(ScanTimeText, out TimeOnly time)
                    ? time
                    : TimeOnly.FromDateTime(DateTime.Now),
                Status = AttendanceStatus,
                OtherInfo = OtherInfo.Trim(),
                MarkedByEmployeeId = currentUser?.EmployeeId
            };
        }

        protected void LoadAttendanceToForm(Attendance attendance)
        {
            SelectedStudentClassId = attendance.StudentClassId;
            AttendanceDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue);
            ScanTimeText = attendance.ScanTime.ToString("HH:mm");
            AttendanceStatus = attendance.Status;
            OtherInfo = attendance.OtherInfo;
        }

        protected bool ValidateRequiredFields()
        {
            if (!SelectedStudentClassId.HasValue)
            {
                _messageService.Show(
                    "សូមមេត្តាជ្រើសរើសសិស្ស និងថ្នាក់!",
                    "ខ្វះព័ត៌មាន!",
                    MessageButton.OK,
                    MessageIcon.Information);
                return false;
            }

            if (!AttendanceDate.HasValue)
            {
                _messageService.Show(
                    "សូមមេត្តាជ្រើសរើសកាលបរិច្ឆេទ!",
                    "ខ្វះព័ត៌មាន!",
                    MessageButton.OK,
                    MessageIcon.Information);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ScanTimeText) || !TimeOnly.TryParse(ScanTimeText, out _))
            {
                _messageService.Show(
                    "សូមមេត្តាបញ្ចូលម៉ោង (HH:mm)!",
                    "ខ្វះព័ត៌មាន!",
                    MessageButton.OK,
                    MessageIcon.Information);
                return false;
            }

            User? currentUser = _authorizationService.CurrentUser;
            if (currentUser == null)
            {
                _messageService.Show("សូមធ្វើការ login ចូលប្រព័ន្ធមួយនេះដើម្បីបន្តការប្រើប្រាស់.", "ឈប់សិន!", MessageButton.OK, MessageIcon.Error);
                return false;
            }

            return true;
        }

        protected async Task GoBackAsync()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;
            if (previous != null)
            {
                await _navigationService.NavigateAsync(previous.GetType());
            }
            else
            {
                await _navigationService.NavigateAsync<AttendanceViewModel>();
            }
        }
    }
}
