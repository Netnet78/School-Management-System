using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Classes.ViewModels
{
    public partial class ClassViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IClassService _classService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

        // ====== Permission properties ======
        [ObservableProperty]
        private bool _canViewClasses;

        [ObservableProperty]
        private bool _canInsertClasses;

        [ObservableProperty]
        private bool _canEditClasses;

        [ObservableProperty]
        private bool _canDeleteClasses;

        [ObservableProperty]
        private bool _canManageDepartments;

        // ====== Class list ======
        [ObservableProperty]
        private ObservableCollection<Class> _classes = [];

        [ObservableProperty]
        private bool _isLoading;

        public ClassViewModel(
            IClassService classService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _classService = classService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public async Task LoadAsync()
        {
            var permissions = await _classService.GetPermissionsAsync();
            CanViewClasses = permissions.CanView;
            CanInsertClasses = permissions.CanInsert;
            CanEditClasses = permissions.CanEdit;
            CanDeleteClasses = permissions.CanDelete;
            CanManageDepartments = permissions.CanManageDepartments;

            await LoadClassesAsync();
        }

        // ====== Load data ======

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadClassesAsync();
        }

        private async Task LoadClassesAsync()
        {
            IsLoading = true;

            try
            {
                ReturnResponse<IEnumerable<Class>> response = await _classService.GetAllAsync(
                    orderBy: [new SortCriteria<Class>("Id")]);

                if (response.Status != Status.Success)
                {
                    _messageService.Show(response.Message ?? "Failed to load classes");
                    return;
                }

                Classes.Clear();
                foreach (Class @class in response.Value ?? [])
                {
                    Classes.Add(@class);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading classes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ====== Navigation to Add / Edit ======

        [RelayCommand]
        private async Task AddClassAsync()
        {
            if (!CanInsertClasses)
            {
                _messageService.Show("អ្នកគ្មានសិទ្ធិបន្ថែមថ្នាក់ថ្មីទេ!", "គ្មានសិទ្ធិ!", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            await _navigationService.NavigateAsync<AddClassViewModel>();
        }

        [RelayCommand]
        private async Task EditClassAsync(Class? cls)
        {
            if (cls == null || !CanEditClasses) return;

            await _navigationService.NavigateAsync<EditClassViewModel>(new EditClassParams { Class = cls });
        }

        [RelayCommand]
        private async Task DeleteClassAsync(Class? cls)
        {
            if (cls == null || !CanDeleteClasses) return;

            MessageResult result = _messageService.Show(
                $"តើអ្នកប្រាកដទេថានឹងលុបថ្នាក់ \"{cls.GetKhmerName()}\"?",
                "បញ្ជាក់ការលុប",
                MessageButton.YesNo,
                MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            IsLoading = true;

            try
            {
                ReturnResponse response = await _classService.DeleteAsync(cls);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានលុបថ្នាក់ដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                    Classes.Remove(cls);
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសក្នុងការលុប!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ====== Student list ======

        [RelayCommand]
        private async Task ShowStudentsAsync(Class? cls)
        {
            if (cls == null) return;

            await _navigationService.NavigateAsync<ClassStudentListViewModel>(new ClassStudentListParams { Class = cls });
        }

        // ====== Navigation to Department management ======

        [RelayCommand]
        private async Task ManageDepartmentsAsync()
        {
            await _navigationService.NavigateAsync<DepartmentViewModel>();
        }
    }
}
