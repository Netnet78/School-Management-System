using SchoolManagement.Core.Shared.Presentation.Contracts;


namespace SchoolManagement.Presentation.Shared.Services
{
    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(action);
        }

        public void Invoke<T>(Func<T> function)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(function);
        }

        public async Task InvokeAsync(Action action)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(action);
        }

        public async Task InvokeAsync<T>(Func<T> function)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(function);
        }
    }
}
