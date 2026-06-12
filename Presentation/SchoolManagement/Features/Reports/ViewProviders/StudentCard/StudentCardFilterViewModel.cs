using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentCard
{
    public partial class StudentCardFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IClassService _classService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IAuthorizationService _authorizationService;

        private const int SearchDebounceDelayMs = 500;
        private Timer? _searchDebounceTimer;

        public ReportTag ReportTypeKey => ReportTag.StudentCard;

        public event Action? FilterChanged;

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        [FilterIgnore]
        [ObservableProperty]
        private string _principalName = string.Empty;

        [FilterIgnore]
        [ObservableProperty]
        private string _signaturePath = string.Empty;

        [FilterIgnore]
        [ObservableProperty]
        private string _location = "វិ.ច.ប.ឯ.ដប.ប៉ប៉";

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        partial void OnPrincipalNameChanged(string value) => ScheduleDebouncedFilter();

        partial void OnLocationChanged(string value) => ScheduleDebouncedFilter();

        partial void OnSignaturePathChanged(string value) => ScheduleDebouncedFilter();

        public StudentCardFilterViewModel(
            IClassService classService,
            IFileDialogService fileDialogService,
            IAuthorizationService authorizationService)
        {
            _classService = classService;
            _fileDialogService = fileDialogService;
            _authorizationService = authorizationService;
        }

        partial void OnSelectedClassChanged(Class? value)
        {
            ScheduleDebouncedFilter();
        }

        partial void OnSearchKeywordChanged(string value)
        {
            ScheduleDebouncedFilter();
        }

        public object GetFilterData()
        {
            return new StudentCardFilter
            {
                ClassId = SelectedClass?.Id,
                PrincipalName = PrincipalName,
                SignaturePath = SignaturePath,
                Location = Location,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
            };
        }

        public async Task LoadAsync()
        {
            User? user = _authorizationService.CurrentUser;

            if (user == null) return;

            if (user.IsValidRole(RoleType.Admin))
            {
                Classes = (await _classService.GetAllAsync()).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.HeadTeacher))
            {
                Classes = (await _classService.GetAllAsync(1, null,
                [new(c => c.Generation.DepartmentId, FilterOperator.Equals, user.Employee?.DepartmentId)],
                includes: ["Generation"])).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.Teacher))
            {
                Classes = (await _classService.GetAllAsync(1, null,
                [new(c => c.TeacherId, FilterOperator.Equals, user.EmployeeId)],
                includes: ["Employee"])).Value ?? [];
            }
        }

        private void ScheduleDebouncedFilter()
        {
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(
                async (_) =>
                {
                    FilterChanged?.Invoke();
                },
                null,
                SearchDebounceDelayMs,
                Timeout.Infinite);
        }

        [RelayCommand]
        private void BrowseSignature()
        {
            var fileDialog = _fileDialogService.ShowDialog(
                "Select principal/admin signature",
                false,
                "Image files",
                "png",
                "jpg",
                "jpeg",
                "bmp");

            if (fileDialog.File == null)
            {
                return;
            }

            SignaturePath = fileDialog.File.FilePath;
        }

        public void ResetFilterData()
        {
            SelectedClass = null;
            PrincipalName = string.Empty;
            SignaturePath = string.Empty;
            Location = "វិ.ច.ប.ឯ.ដប.ប៉ប៉";
            SearchKeyword = string.Empty;
        }
    }
}
