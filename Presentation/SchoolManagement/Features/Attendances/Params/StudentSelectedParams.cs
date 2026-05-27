namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public sealed class StudentSelectedParams : INavigationParams
    {
        public int StudentClassId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }
}
