namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface IDispatcherService
    {
        public Task InvokeAsync(Action action);
        public Task InvokeAsync<T>(Func<T> function);
        public void Invoke(Action action);
        public void Invoke<T>(Func<T> function);
    }
}
