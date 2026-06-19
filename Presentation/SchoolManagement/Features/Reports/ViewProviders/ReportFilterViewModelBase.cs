using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders
{
    public abstract class ReportFilterViewModelBase<TFilter> : ObservableObject, IReportFilterViewModel, IAsyncLoadable, IDisposable
        where TFilter : new()
    {
        public abstract string ReportTypeKey { get; }
        public event Action? FilterChanged;

        private Timer? _debounceTimer;

        protected void ScheduleDebouncedFilter(int delayMs = 400)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                async _ =>
                {
                    try
                    {
                        FilterChanged?.Invoke();
                    }
                    catch
                    {
                        throw;
                    }
                },
                null, delayMs, Timeout.Infinite);
        }

        protected void OnFilterChanged() => FilterChanged?.Invoke();

        public abstract TFilter GetFilterData();

        object IReportFilterViewModel.GetFilterData() => GetFilterData()!;

        public abstract void ResetFilterData();

        public virtual Task LoadAsync() => Task.CompletedTask;

        public void Dispose()
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
            GC.SuppressFinalize(this);
        }
    }
}
