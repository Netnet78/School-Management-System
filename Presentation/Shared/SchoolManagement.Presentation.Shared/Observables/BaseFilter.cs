using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Observables
{
    public partial class BaseFilter : ObservableObject
    {
        [ObservableProperty]
        private int? _pageSize;
        [ObservableProperty]
        private int? _currentPage;
        [ObservableProperty]
        private OrderDirection? _orderBy;
    }
}
