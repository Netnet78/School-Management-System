using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Features.Subjects.Observables
{
    public partial class ComponentEntry : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;
        [ObservableProperty]
        public string _khmerName = string.Empty;

        public ComponentEntry() { }

        public ComponentEntry(string name, string khmerName)
        {
            Name = name;
            KhmerName = khmerName;
        }
    }
}