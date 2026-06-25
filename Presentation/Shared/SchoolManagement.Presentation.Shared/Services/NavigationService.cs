using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace SchoolManagement.Presentation.Shared.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDispatcherService _dispatcherService;
        private readonly Dictionary<Type, IViewModel> _vmCache = new();

        public NavigationService(IServiceProvider serviceProvider, IDispatcherService dispatcherService)
        {
            _serviceProvider = serviceProvider;
            _dispatcherService = dispatcherService;
        }

        public IViewModel? CurrentViewModel { get; private set; }
        public IViewModel? PreviousViewModel { get; private set; }

        public event Action<IViewModel?, IViewModel>? OnViewModelChanged;
        public async Task NavigateAsync<TViewModel>(INavigationParams? @params = null) where TViewModel : IViewModel
        {
            await NavigateAsync(typeof(TViewModel), @params).ConfigureAwait(false);
        }

        public async Task NavigateAsync(Type viewModelType, INavigationParams? @params = null)
        {
            if (!_vmCache.TryGetValue(viewModelType, out IViewModel? viewmodel))
            {
                viewmodel = (IViewModel)_serviceProvider.GetRequiredService(viewModelType);
                _vmCache[viewModelType] = viewmodel;
            }

            IViewModel? previous = CurrentViewModel;

            await _dispatcherService.InvokeAsync(() =>
            {
                PreviousViewModel = previous;
                CurrentViewModel = viewmodel;
                OnViewModelChanged?.Invoke(previous, viewmodel);
            });

            if (viewmodel is INavigationAware aware && @params != null)
            {
                await aware.OnNavigatedToAsync(@params).ConfigureAwait(false);
            }

            if (viewmodel is IAsyncLoadable loadable)
            {
                await loadable.LoadAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task<bool?> OpenDialog<TViewModel, TView>(Type? vm = null)
            where TViewModel : IViewModel
            where TView : IDialogWindow
        {
            TViewModel viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            TView view = _serviceProvider.GetRequiredService<TView>();

            try
            {
                return view.OpenDialog(viewModel);
            }
            finally
            {
                if (vm != null && Application.Current is { } app &&
                    app.Dispatcher != null &&
                    !app.Dispatcher.HasShutdownStarted &&
                    !app.Dispatcher.HasShutdownFinished)
                {
                    await NavigateAsync(vm);
                }
            }
        }

        public void ClearCache()
        {
            _vmCache.Clear();
        }
    }
}
