using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public partial class SelectStudentViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IStudentClassService _studentClassService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private int _classId;
        private string _className = string.Empty;

        [ObservableProperty]
        private ObservableCollection<StudentClass> _students = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canManageAttendances;

        [ObservableProperty]
        private string _stepTitle = string.Empty;

        public SelectStudentViewModel(
            IStudentClassService studentClassService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _studentClassService = studentClassService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is ClassSelectedParams p)
            {
                _classId = p.ClassId;
                _className = p.ClassName;
            }

            return Task.CompletedTask;
        }

        public async Task LoadAsync()
        {
            if (_classId == 0)
            {
                return;
            }

            IsLoading = true;

            try
            {
                ReturnResponse permission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageAttendances);
                if (permission.Status != Status.Success)
                {
                    CanManageAttendances = false;
                    _messageService.Show(permission.Message ?? "You do not have permission.", "Access denied", MessageButton.OK, MessageIcon.Hand);
                    return;
                }

                CanManageAttendances = true;
                StepTitle = $"ថ្នាក់: {_className}";

                ReturnResponse<IEnumerable<StudentClass>> response = await _studentClassService.GetAllAsync(
                    filters: [
                        new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, _classId),
                        new FilterCondition<StudentClass>(sc => sc.IsActive, FilterOperator.Equals, true)
                    ],
                    page: 1,
                    pageSize: int.MaxValue,
                    orderBy: [new SortCriteria<StudentClass>(s => s.Student.Candidate.FullName)],
                    includes: ["Student", "Student.Candidate"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    Students.Clear();
                    foreach (StudentClass sc in response.Value)
                    {
                        Students.Add(sc);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"Error loading students: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SelectStudentAsync(StudentClass? sc)
        {
            if (sc == null || !CanManageAttendances)
            {
                return;
            }

            await _navigationService.NavigateAsync<WizardAddAttendanceViewModel>(new StudentSelectedParams
            {
                StudentClassId = sc.Id,
                StudentName = sc.Student.FullName,
                ClassName = _className
            });
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await _navigationService.NavigateAsync<AttendanceViewModel>();
        }
    }
}
