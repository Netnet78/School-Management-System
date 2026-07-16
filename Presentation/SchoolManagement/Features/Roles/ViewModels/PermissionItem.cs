using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Features.Roles.ViewModels
{
    public partial class PermissionItem : ObservableObject
    {
        public Permission Permission { get; }

        [ObservableProperty]
        private bool _isAssigned;

        public string Name => Permission.Name;

        public PermissionItem(Permission permission, bool isAssigned)
        {
            Permission = permission;
            _isAssigned = isAssigned;
        }
    }
}
