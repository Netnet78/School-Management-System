using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Features.Employees.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {


        public Task LoadAsync()
        {
            throw new NotImplementedException();
        }
    }
}

