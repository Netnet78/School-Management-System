namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface INavigationAware
    {
        public Task OnNavigatedToAsync(INavigationParams @params);
    }
}
