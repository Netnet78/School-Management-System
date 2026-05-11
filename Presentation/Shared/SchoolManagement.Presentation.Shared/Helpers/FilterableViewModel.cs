using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Shared.Helpers
{
    public abstract class FilterableViewModel<T>
    {
        protected ObservableCollection<T> _items = new();
        public ICollectionView ItemsView { get; }

        protected FilterableViewModel()
        {
            ItemsView = CollectionViewSource.GetDefaultView(_items);
            ItemsView.Filter = FilterPredicate;
        }

        protected abstract bool FilterPredicate(object item);

        public void RefreshFilter()
        {
            ItemsView.Refresh();
        }
    }
}
