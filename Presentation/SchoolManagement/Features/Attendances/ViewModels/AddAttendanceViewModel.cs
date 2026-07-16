using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public partial class AddAttendanceViewModel : AttendanceFormViewModelBase
    {
        private readonly IAttendanceService _attendanceService;

        public AddAttendanceViewModel(
            IAttendanceService attendanceService,
            IStudentClassService studentClassService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
            : base(studentClassService, authorizationService, messageService, navigationService)
        {
            _attendanceService = attendanceService;
        }

        public override async Task LoadAsync()
        {
            IsLoading = true;

            try
            {
                await LoadCommonAsync();
                if (!CanManageAttendances)
                {
                    return;
                }
                ResetForm();
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

            if (!ValidateRequiredFields())
            {
                return;
            }

            IsLoading = true;

            try
            {
                Attendance attendance = BuildAttendanceFromForm();
                ReturnResponse response = await _attendanceService.InsertAsync(attendance);

                if (response.Status != Status.Success)
                {
                    _messageService.Show(response.Message ?? "Unable to save the attendance.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                _messageService.Show("Attendance added successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while adding the attendance: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
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
