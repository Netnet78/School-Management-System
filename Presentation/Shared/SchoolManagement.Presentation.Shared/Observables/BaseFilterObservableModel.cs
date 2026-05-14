using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Observables
{
    public partial class BaseFilterObservableModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime? _fromDate;
        [ObservableProperty]
        private DateTime? _toDate;
        [ObservableProperty]
        private int _currentPage;
        [ObservableProperty]
        private int _pageSize;
        [ObservableProperty]
        private OrderType _orderBy;
    }
}
