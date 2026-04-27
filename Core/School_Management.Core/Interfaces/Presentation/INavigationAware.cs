namespace School_Management.Core.Interfaces.Presentation
{
    public interface INavigationAware
    {
        public Task OnNavigatedToAsync(INavigationParams @params);
    }
}
