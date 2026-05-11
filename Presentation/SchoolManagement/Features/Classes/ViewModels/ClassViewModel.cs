using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Policies;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Helpers;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Presentation.Contracts;
using SchoolManagement.Core.Shared.Models;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SchoolManagement.Presentation.ViewModels
{
    public partial class ClassViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly IClassService _classService;
        private readonly IUserSessionService _userSessionService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;

        [ObservableProperty]
        private ObservableCollection<Class> _classes = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canViewClasses;

        public ClassViewModel(
            IClassService classService,
            IUserSessionService userSessionService,
            IAuthorizationService authorizationService,
            IMessageService messageService)
        {
            _classService = classService;
            _userSessionService = userSessionService;
            _authorizationService = authorizationService;
            _messageService = messageService;
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadClassesAsync();
        }

        public async Task LoadAsync()
        {
            await LoadClassesAsync();
        }

        private async Task LoadClassesAsync()
        {
            IsLoading = true;

            try
            {
                User? currentUser = _userSessionService.CurrentUser;
                if (currentUser == null)
                {
                    _messageService.Show("Current user session hasn't been set!");
                    return;
                }

                await CanProceed(null, PermissionType.ViewClasses);

                Expression<Func<Class, bool>> predicate = ClassFilters.BuildAccessFilter(currentUser);
                ReturnResponse<IEnumerable<Class>> response = await _classService.GetAllAsync(
                    predicate: predicate,
                    orderBy: query => query.OrderBy(c => c.Id));

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

        private async Task CanProceed(Class? entity, params PermissionType[] permissions)
        {
            ReturnResponse response = await _authorizationService.AuthorizeAsync(entity, OperatorMode.AND, permissions);

            CanViewClasses = response.Status == Status.Success;

            if (response.Status != Status.Success)
            {
                _messageService.Show(
                    response.Message,
                    "មិនអនុញ្ញាតជាដាច់ខាត!",
                    MessageButton.OK,
                    MessageIcon.Exclamation);
            }
        }
    }
}
