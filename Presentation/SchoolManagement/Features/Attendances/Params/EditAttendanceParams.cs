namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public sealed class EditAttendanceParams : INavigationParams
    {
        public Attendance? Attendance { get; set; }
    }
}
