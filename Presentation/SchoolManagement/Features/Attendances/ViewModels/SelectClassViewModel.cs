using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public partial class SelectClassViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<Class> _classes = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canManageAttendances;

        [ObservableProperty]
        private bool _hasClasses;

        public SelectClassViewModel(
            IClassService classService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _classService = classService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public async Task LoadAsync()
        {
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

                ReturnResponse<IEnumerable<Class>> response = await _classService.GetAllAsync(
                    filters: null,
                    page: 1,
                    pageSize: int.MaxValue,
                    orderBy: [new(c => c.Grade.KhmerName)],
                    includes: ["Grade", "Generation", "Generation.Department", "Teacher"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    Classes.Clear();
                    foreach (Class cls in response.Value)
                    {
                        Classes.Add(cls);
                    }
                    HasClasses = Classes.Count > 0;
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"Error loading classes: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SelectClassAsync(Class? cls)
        {
            if (cls == null || !CanManageAttendances)
            {
                return;
            }

            await _navigationService.NavigateAsync<SelectStudentViewModel>(new ClassSelectedParams
            {
                ClassId = cls.Id,
                ClassName = cls.KhmerName
            });
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await _navigationService.NavigateAsync<AttendanceViewModel>();
        }
    }
}
