namespace SchoolManagement.Presentation.Shared.Contracts
{
    /// <summary>
    /// An interface for defining a view model that will be notified and takes a parameter like a web page
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// A function that runs before the <see cref="IAsyncLoadable.LoadAsync"/> in the navigation service
        /// and the navigated view model will be given the <paramref name="params"/> value that is inherited from
        /// <see cref="INavigationParams"/>
        /// </summary>
        /// <param name="params"></param>
        /// <returns></returns>
        public Task OnNavigatedToAsync(INavigationParams @params);
    }
}
