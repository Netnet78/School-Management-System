namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface INavigationService
    {
        event Action<IViewModel?, IViewModel>? OnViewModelChanged;
        IViewModel? CurrentViewModel { get; }
        IViewModel? PreviousViewModel { get; }
        Task NavigateAsync<TViewModel>(INavigationParams? @params = null) where TViewModel : IViewModel;
        Task NavigateAsync(Type viewModelType, INavigationParams? @params = null);

        /// <summary>
        /// Opens a dialog window for the specified view model and returns the dialog result.
        /// </summary>
        /// <remarks>This method asynchronously navigates to the view model's type after the dialog is
        /// closed. The dialog is shown modally and blocks input to other windows until closed.</remarks>
        /// <typeparam name="TViewModel">The type of the view model to associate with the dialog. Must implement <see cref="IViewModel"/>.</typeparam>
        /// <typeparam name="TView">The type of the dialog window to display. Must implement <see cref="IDialogWindow"/>.</typeparam>
        /// <param name="vm">The view model instance to use as the data context for the dialog window. Cannot be null.</param>
        /// <returns>A <see langword="true"/> value if the dialog was accepted; <see langword="false"/> if it was canceled; or
        /// <see langword="null"/> if no result was returned.</returns>
        Task<bool?> OpenDialog<TViewModel, TView>(Type? vm = null)
            where TViewModel : IViewModel
            where TView : IDialogWindow;
        void ClearCache();
    }
}
