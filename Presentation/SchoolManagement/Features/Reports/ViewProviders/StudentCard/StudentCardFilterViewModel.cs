using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Infrastructure.Features.Classes.Contracts;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentCard
{
    public partial class StudentCardFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IClassRepository _classRepository;

        public string ReportTypeKey => "student-card";

        public event Action? FilterChanged;

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        public StudentCardFilterViewModel(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public object GetFilterData()
        {
            return new StudentCardFilter
            {
                ClassId = SelectedClass?.Id,
            };
        }

        public async Task LoadAsync()
        {
            Classes = (await _classRepository.GetAllAsync()).ToList();
        }

        partial void OnSelectedClassChanged(Class? value)
        {
            FilterChanged?.Invoke();
        }
    }
}
