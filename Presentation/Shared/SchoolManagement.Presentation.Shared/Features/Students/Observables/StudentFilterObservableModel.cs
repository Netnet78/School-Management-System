using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Features.Students.Observables
{
    public partial class StudentFilterObservableModel : ObservableObject
    {
        [ObservableProperty]
        private string? _search;

        [ObservableProperty]
        private Gender? _gender;

        [ObservableProperty]
        private string? _sortBy;

        [ObservableProperty]
        private OrderDirection _orderBy = OrderDirection.Descending;

        [ObservableProperty]
        private DateTime? _fromDate;

        [ObservableProperty]
        private DateTime? _toDate;

        [ObservableProperty]
        private StudentStayType? _stayType;

        [ObservableProperty]
        private bool _showInactiveStudents;

        [ObservableProperty]
        private int? _departmentId;

        [ObservableProperty]
        private int? _generationId;

        public IEnumerable<SortCriteria<Student>> BuildOrder()
        {
            return SortBy switch
            {
                "Id" => [new SortCriteria<Student>("Id", OrderBy)],
                "FullName" => [new SortCriteria<Student>("Candidate.FullName", OrderBy)],
                "CreatedAt" => [new SortCriteria<Student>("CreatedAt", OrderBy)],
                _ => [new SortCriteria<Student>("Id", OrderBy)],
            };
        }

        public List<FilterCondition<Student>> BuildFilters()
        {
            bool? isActive = ShowInactiveStudents ? null : true;

            List<FilterCondition<Student>> filters = [];

            if (!string.IsNullOrWhiteSpace(Search))
            {
                filters.Add(new(s => s.Candidate.FullName, FilterOperator.Contains, Search));
            }

            if (Gender.HasValue)
            {
                filters.Add(new(s => s.Candidate.Gender, FilterOperator.Equals, Gender.Value));
            }

            if (FromDate.HasValue)
            {
                filters.Add(new(s => s.Candidate.CreatedAt, FilterOperator.GreaterThanOrEqual, FromDate.Value));
            }

            if (ToDate.HasValue)
            {
                filters.Add(new(s => s.Candidate.CreatedAt, FilterOperator.LessThanOrEqual, ToDate.Value));
            }

            if (isActive.HasValue)
            {
                filters.Add(new(s => s.IsActive, FilterOperator.Equals, isActive.Value));
            }

            if (StayType.HasValue)
            {
                filters.Add(new(s => s.Candidate.StayType, FilterOperator.Equals, StayType.Value));
            }

            if (DepartmentId.HasValue)
            {
                filters.Add(new(s => s.Candidate.Skill.Department.Id, FilterOperator.Equals, DepartmentId.Value));
            }

            if (GenerationId.HasValue)
            {
                filters.Add(new FilterCondition<Student>(
                    s => s.Classes.Any(sc => sc.Class.GenerationId == GenerationId.Value)));
            }

            return filters;
        }
    }
}
