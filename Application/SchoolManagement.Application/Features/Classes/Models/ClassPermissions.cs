namespace SchoolManagement.Application.Features.Classes.Models
{
    public record ClassPermissions
    {
        public bool CanView { get; init; }
        public bool CanInsert { get; init; }
        public bool CanEdit { get; init; }
        public bool CanDelete { get; init; }
        public bool CanManageDepartments { get; init; }
        public User? CurrentUser { get; init; }
    }
}
