namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface INavigationService
    {
        event Action<IViewModel?, IViewModel>? OnViewModelChanged;
        IViewModel? CurrentViewModel { get; }
        IViewModel? PreviousViewModel { get; }
        Task NavigateAsync<TViewModel>(INavigationParams? @params = null) where TViewModel : IViewModel;
        Task NavigateAsync(Type viewModelType, INavigationParams? @params = null);
    }
}
