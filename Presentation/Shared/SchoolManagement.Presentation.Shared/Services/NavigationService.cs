using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Core.Shared.Presentation.Contracts;

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

            if (viewmodel is IAsyncLoadable loadable)
            {
                await loadable.LoadAsync();
            }

            if (viewmodel is INavigationAware aware && @params != null)
            {
                await aware.OnNavigatedToAsync(@params);
            }

            await _dispatcherService.InvokeAsync(async () =>
            {
                PreviousViewModel = CurrentViewModel;
                CurrentViewModel = viewmodel;
                OnViewModelChanged?.Invoke(PreviousViewModel, CurrentViewModel);
            });

            await Task.CompletedTask;
        }
    }
}
