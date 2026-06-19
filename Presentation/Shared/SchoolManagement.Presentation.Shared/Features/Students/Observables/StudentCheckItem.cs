using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Features.Students.Observables
{
    public partial class StudentCheckItem : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected = false;

        [ObservableProperty]
        private Student? _student;
    }
}
