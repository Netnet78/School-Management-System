using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public partial class EditAttendanceViewModel : AttendanceFormViewModelBase, INavigationAware
    {
        private readonly IAttendanceService _attendanceService;
        private Attendance? _attendance;

        public EditAttendanceViewModel(
            IAttendanceService attendanceService,
            IStudentClassService studentClassService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
            : base(studentClassService, authorizationService, messageService, navigationService)
        {
            _attendanceService = attendanceService;
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is not EditAttendanceParams p)
            {
                return;
            }

            _attendance = p.Attendance;
        }

        public override async Task LoadAsync()
        {
            if (_attendance == null)
            {
                return;
            }

            IsLoading = true;

            try
            {
                await LoadCommonAsync();
                if (!CanManageAttendances)
                {
                    return;
                }
                LoadAttendanceToForm(_attendance);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!CanManageAttendances || _attendance == null)
            {
                return;
            }

            if (!ValidateRequiredFields())
            {
                return;
            }

            IsLoading = true;

            try
            {
                Attendance attendance = BuildAttendanceFromForm();
                attendance.Id = _attendance.Id;

                ReturnResponse response = await _attendanceService.UpdateAsync(attendance);
                if (response.Status != Status.Success)
                {
                    _messageService.Show(response.Message ?? "Unable to save the attendance.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                _messageService.Show("Attendance updated successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while updating the attendance: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await GoBackAsync();
        }
    }
}
