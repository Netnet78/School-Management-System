using CommunityToolkit.Mvvm.ComponentModel;
using School_Management.Core.Interfaces;
using School_Management.Core.Interfaces.Presentation;

namespace School_Management.Presentation.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {


        public Task LoadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
