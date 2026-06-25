using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public partial class AddStudentOptionViewModel : ObservableObject, IViewModel
    {
        private readonly INavigationService navigationService;

        public AddStudentOptionViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private async Task GoToAddStudentView()
        {
            await navigationService.NavigateAsync<AddStudentViewModel>();
        }

        [RelayCommand]
        private async Task GoToAssignCandidateView()
        {
            await navigationService.NavigateAsync<AssignCandidateViewModel>();
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await navigationService.NavigateAsync<StudentListViewModel>();
        }
    }
}
