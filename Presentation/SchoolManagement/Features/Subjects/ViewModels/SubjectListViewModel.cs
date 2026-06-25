using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Presentation.Features.Dashboard.ViewModels;
using SchoolManagement.Presentation.Shared.Features.Subjects.Params;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Subjects.ViewModels
{
    public partial class SubjectListViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly ISubjectService _subjectService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IAuthorizationService _authorizationService;

        [ObservableProperty]
        private bool _canManageSubjects;

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public SubjectListViewModel(
            ISubjectService subjectService,
            IMessageService messageService,
            INavigationService navigationService,
            IAuthorizationService authorizationService)
        {
            _subjectService = subjectService;
            _messageService = messageService;
            _navigationService = navigationService;
            _authorizationService = authorizationService;
        }

        public async Task LoadAsync()
        {
            var response = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageSubjects);
            CanManageSubjects = response.Status == Status.Success;

            await LoadSubjectsAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadSubjectsAsync();
        }

        private async Task LoadSubjectsAsync()
        {
            IsLoading = true;

            try
            {
                var response = await _subjectService.GetAllAsync(
                    orderBy: [new SortCriteria<Subject>("Id")]);

                if (response.Status != Status.Success)
                {
                    _messageService.Show(response.Message ?? "មិនអាចទាញទិន្នន័យមុខវិជ្ជាបានទេ!");
                    return;
                }

                Subjects.Clear();
                foreach (var subject in response.Value ?? [])
                {
                    Subjects.Add(subject);
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

        async partial void OnSearchTextChanged(string value)
        {
            await LoadSubjectsAsync();
        }

        [RelayCommand]
        private async Task AddSubjectAsync()
        {
            if (!CanManageSubjects)
            {
                _messageService.Show("អ្នកគ្មានសិទ្ធិបន្ថែមមុខវិជ្ជាទេ!", "គ្មានសិទ្ធិ!", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            await _navigationService.NavigateAsync<AddSubjectViewModel>();
        }

        [RelayCommand]
        private async Task EditSubjectAsync(Subject? subject)
        {
            if (subject == null || !CanManageSubjects) return;

            await _navigationService.NavigateAsync<EditSubjectViewModel>(new EditSubjectNavigationParams { Subject = subject });
        }

        [RelayCommand]
        private async Task DeleteSubjectAsync(Subject? subject)
        {
            if (subject == null || !CanManageSubjects) return;

            MessageResult result = _messageService.Show(
                $"តើអ្នកប្រាកដទេថានឹងលុបមុខវិជ្ជា \"{subject.KhmerName}\"?",
                "បញ្ជាក់ការលុប",
                MessageButton.YesNo,
                MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            IsLoading = true;

            try
            {
                var response = await _subjectService.DeleteAsync(subject);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានលុបមុខវិជ្ជាដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                    Subjects.Remove(subject);
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

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await _navigationService.NavigateAsync<DashboardViewModel>();
        }
    }
}
