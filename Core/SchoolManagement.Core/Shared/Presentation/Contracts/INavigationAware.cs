namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface INavigationAware
    {
        public Task OnNavigatedToAsync(INavigationParams @params);
    }
}
