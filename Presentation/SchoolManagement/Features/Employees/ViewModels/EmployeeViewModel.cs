using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Shared.Presentation.Contracts;

namespace SchoolManagement.Presentation.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {


        public Task LoadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
