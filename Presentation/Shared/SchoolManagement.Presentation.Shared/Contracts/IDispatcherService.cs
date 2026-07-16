namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface IDispatcherService
    {
        public Task InvokeAsync(Action action);
        public Task<T> InvokeAsync<T>(Func<T> function);
        public void Invoke(Action action);
        public T Invoke<T>(Func<T> function);
    }
}
