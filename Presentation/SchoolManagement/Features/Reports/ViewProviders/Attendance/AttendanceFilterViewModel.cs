using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Application.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.Attendance
{
    public partial class AttendanceFilterViewModel : ReportFilterViewModelBase<AttendanceReportFilter>
    {
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;

        public override string ReportTypeKey => "attendance-report";

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        public ObservableCollection<SelectableClass> SelectableClasses { get; } = [];

        [ObservableProperty]
        private bool _isMonthlyMode = true;

        // Monthly mode fields
        [ObservableProperty]
        private int _selectedMonth = DateTime.UtcNow.Month;

        [ObservableProperty]
        private int _selectedYear = DateTime.UtcNow.Year;

        // Custom date range mode fields
        [ObservableProperty]
        private DateTime _dateFrom = new(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [ObservableProperty]
        private DateTime _dateTo = DateTime.UtcNow;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        public List<MonthOption> Months { get; } =
        [
            new(1, "មករា"),
            new(2, "កុម្ភៈ"),
            new(3, "មីនា"),
            new(4, "មេសា"),
            new(5, "ឧសភា"),
            new(6, "មិថុនា"),
            new(7, "កក្កដា"),
            new(8, "សីហា"),
            new(9, "កញ្ញា"),
            new(10, "តុលា"),
            new(11, "វិច្ឆិកា"),
            new(12, "ធ្នូ"),
        ];

        public List<int> Years { get; } = Enumerable.Range(DateTime.UtcNow.Year - 5, 11).ToList();

        public AttendanceFilterViewModel(IClassService classService, IAuthorizationService authorizationService)
        {
            _classService = classService;
            _authorizationService = authorizationService;
            SelectableClasses.CollectionChanged += OnSelectableClassesChanged;
        }

        public override AttendanceReportFilter GetFilterData()
        {
            var classIds = SelectableClasses
                .Where(sc => sc.IsSelected)
                .Select(sc => sc.Class.Id)
                .ToList();

            if (IsMonthlyMode)
            {
                return new()
                {
                    ClassIds = classIds.Count > 0 ? classIds : null,
                    Month = SelectedMonth,
                    Year = SelectedYear,
                    SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
                };
            }

            return new()
            {
                ClassIds = classIds.Count > 0 ? classIds : null,
                DateFrom = DateFrom,
                DateTo = DateTo,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
            };
        }

        public override async Task LoadAsync()
        {
            User? user = _authorizationService.CurrentUser;
            if (user == null) return;

            IEnumerable<Class> loadedClasses;
            if (user.IsValidRole(RoleType.Admin))
            {
                loadedClasses = (await _classService.GetAllAsync()).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.HeadTeacher))
            {
                loadedClasses = (await _classService.GetAllAsync(
                    filters: [new(c => c.Generation.DepartmentId, FilterOperator.Equals, user.Employee?.DepartmentId)],
                    includes: ["Generation"])).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.Teacher))
            {
                loadedClasses = (await _classService.GetAllAsync(
                    filters: [new(c => c.TeacherId, FilterOperator.Equals, user.EmployeeId)],
                    includes: ["Employee"])).Value ?? [];
            }
            else
            {
                loadedClasses = [];
            }

            Classes = loadedClasses;

            SelectableClasses.CollectionChanged -= OnSelectableClassesChanged;
            SelectableClasses.Clear();
            SelectableClasses.CollectionChanged += OnSelectableClassesChanged;
            foreach (var cls in loadedClasses)
                SelectableClasses.Add(new SelectableClass(cls));
        }

        private void OnSelectableClassesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SelectableClass item in e.NewItems)
                    item.PropertyChanged += (_, _) => OnFilterChanged();
            }
        }

        partial void OnSelectedMonthChanged(int value) => OnFilterChanged();
        partial void OnSelectedYearChanged(int value) => OnFilterChanged();
        partial void OnIsMonthlyModeChanged(bool value) => OnFilterChanged();
        partial void OnDateFromChanged(DateTime value) => OnFilterChanged();
        partial void OnDateToChanged(DateTime value) => OnFilterChanged();
        partial void OnSearchKeywordChanged(string value) => ScheduleDebouncedFilter();

        public override void ResetFilterData()
        {
            foreach (var sc in SelectableClasses)
                sc.IsSelected = false;
            IsMonthlyMode = true;
            SelectedMonth = DateTime.UtcNow.Month;
            SelectedYear = DateTime.UtcNow.Year;
            DateFrom = new(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTo = DateTime.UtcNow;
            SearchKeyword = string.Empty;
        }
    }

    public partial class SelectableClass : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        public Class Class { get; }

        public SelectableClass(Class cls, bool isSelected = false)
        {
            Class = cls;
            _isSelected = isSelected;
        }
    }

    public record MonthOption(int Value, string KhmerName)
    {
        public override string ToString() => KhmerName;
    }
}
