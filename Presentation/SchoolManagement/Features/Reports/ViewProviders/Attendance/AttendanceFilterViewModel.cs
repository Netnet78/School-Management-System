using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Infrastructure.Features.Classes.Contracts;
using SchoolManagement.Infrastructure.Features.Shared.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.Attendance
{
    public partial class AttendanceFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IClassRepository _classRepository;

        public string ReportTypeKey => "attendance-report";

        public event Action? FilterChanged;

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        [ObservableProperty]
        private DateTime _dateFrom = new(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [ObservableProperty]
        private DateTime _dateTo = DateTime.UtcNow;

        public AttendanceFilterViewModel(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public object GetFilterData()
        {
            return new AttendanceReportFilter
            {
                ClassId = SelectedClass?.Id,
                DateFrom = DateFrom,
                DateTo = DateTo,
            };
        }

        public async Task LoadAsync()
        {
            Classes = (await _classRepository.GetAllAsync()).ToList();
        }

        partial void OnSelectedClassChanged(Class? value) => FilterChanged?.Invoke();
        partial void OnDateFromChanged(DateTime value) => FilterChanged?.Invoke();
        partial void OnDateToChanged(DateTime value) => FilterChanged?.Invoke();
    }
}
