using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Observables
{
    public partial class StudentFilterObservableModel : ObservableObject
    {
        private string? search;
        private StudentField? searchField;
        private StudentDataStateFilterOptions dataState = StudentDataStateFilterOptions.All;
        private bool? isActive;
        private Gender? gender;
        private DateTime? fromDate;
        private DateTime? toDate;
        private string? sortBy;
        private StudentStayType? stayType;
        private OrderType orderBy = OrderType.Descending;

        public StudentFilterOptions ToFilterOptions()
        {
            return new()
            {
                Search = Search,
                DataState = DataState,
                FromDate = FromDate,
                ToDate = ToDate,
                Gender = Gender,
                IsActive = IsActive,
                OrderBy = OrderBy,
                SearchField = SearchField,
                SortBy = SortBy,
                StayType = StayType
            };
        }

        public void Reset()
        {
            Search = null;
            DataState = StudentDataStateFilterOptions.All;
            FromDate = null;
            ToDate = null;
            Gender = null;
            IsActive = null;
            OrderBy = OrderType.Descending;
            SearchField = null;
            SortBy = null;
            StayType = null;
        }

        public string? Search
        {
            get => search;
            set
            {
                if (search != value)
                {
                    search = value;
                    OnPropertyChanged(nameof(Search));
                }
            }
        }

        public StudentField? SearchField
        {
            get => searchField;
            set
            {
                if (searchField != value)
                {
                    searchField = value;
                    OnPropertyChanged(nameof(SearchField));
                }
            }
        }

        public StudentDataStateFilterOptions DataState
        {
            get => dataState;
            set
            {
                if (dataState != value)
                {
                    dataState = value;
                    OnPropertyChanged(nameof(DataState));
                }
            }
        }

        public bool? IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public Gender? Gender
        {
            get => gender;
            set
            {
                if (gender != value)
                {
                    gender = value;
                    OnPropertyChanged(nameof(Gender));
                }
            }
        }

        public DateTime? FromDate
        {
            get => fromDate;
            set
            {
                if (fromDate != value && value < ToDate)
                {
                    fromDate = value;
                    OnPropertyChanged(nameof(FromDate));
                }
            }
        }

        public DateTime? ToDate
        {
            get => toDate;
            set
            {
                if (toDate != value && value > FromDate)
                {
                    toDate = value;
                    OnPropertyChanged(nameof(ToDate));
                }
            }
        }

        public string? SortBy
        {
            get => sortBy;
            set
            {
                if (sortBy != value)
                {
                    sortBy = value;
                    OnPropertyChanged(nameof(SortBy));
                }
            }
        }

        public StudentStayType? StayType
        {
            get => stayType;
            set
            {
                if (stayType != value)
                {
                    stayType = value;
                    OnPropertyChanged(nameof(StayType));
                }
            }
        }

        public OrderType OrderBy
        {
            get => orderBy;
            set
            {
                if (orderBy != value)
                {
                    orderBy = value;
                    OnPropertyChanged(nameof(OrderBy));
                }
            }
        }
    }
}
