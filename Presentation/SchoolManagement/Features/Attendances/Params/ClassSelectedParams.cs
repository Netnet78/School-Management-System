namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public sealed class ClassSelectedParams : INavigationParams
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }
}
