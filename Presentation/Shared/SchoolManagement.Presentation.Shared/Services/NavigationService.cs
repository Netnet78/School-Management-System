using Microsoft.Extensions.DependencyInjection;

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
        public Task NavigateAsync<TViewModel>(INavigationParams? @params = null) where TViewModel : IViewModel
        {
            return NavigateAsync(typeof(TViewModel), @params);
        }

        public async Task NavigateAsync(Type viewModelType, INavigationParams? @params = null)
        {
            if (!_vmCache.TryGetValue(viewModelType, out IViewModel? viewmodel))
            {
                viewmodel = (IViewModel)_serviceProvider.GetRequiredService(viewModelType);
                _vmCache[viewModelType] = viewmodel;
            }

            if (viewmodel is INavigationAware aware && @params != null)
            {
                await aware.OnNavigatedToAsync(@params);
            }

            if (viewmodel is IAsyncLoadable loadable)
            {
                await loadable.LoadAsync();
            }

            await _dispatcherService.InvokeAsync(async () =>
            {
                PreviousViewModel = CurrentViewModel;
                CurrentViewModel = viewmodel;
                OnViewModelChanged?.Invoke(PreviousViewModel, CurrentViewModel);
            });

            await Task.CompletedTask;
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
                if (vm != null)
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
