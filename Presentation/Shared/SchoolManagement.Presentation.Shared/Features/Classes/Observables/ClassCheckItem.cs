using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Features.Classes.Models;

namespace SchoolManagement.Presentation.Shared.Features.Classes.Observables
{
    public partial class ClassCheckItem : ObservableObject
    {
        [ObservableProperty]
        private bool _isChecked;

        [ObservableProperty]
        private Class? _class;
    }
}
