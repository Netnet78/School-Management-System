namespace SchoolManagement.Presentation.Shared.Services
{
    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(action);
        }

        public T Invoke<T>(Func<T> function)
        {
            return System.Windows.Application.Current.Dispatcher.Invoke(function);
        }

        public async Task InvokeAsync(Action action)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(action);
        }

        public async Task<T> InvokeAsync<T>(Func<T> function)
        {
            return await System.Windows.Application.Current.Dispatcher.InvokeAsync(function);
        }
    }
}
