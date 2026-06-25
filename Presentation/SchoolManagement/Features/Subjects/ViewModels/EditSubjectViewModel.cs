using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Presentation.Shared.Features.Subjects.Params;

namespace SchoolManagement.Presentation.Features.Subjects.ViewModels
{
    public partial class EditSubjectViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly ISubjectService _subjectService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

        private Subject _subject = null!;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _khmerName = string.Empty;

        [ObservableProperty]
        private decimal _maxScore;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _isSaving;

        public EditSubjectViewModel(
            ISubjectService subjectService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _subjectService = subjectService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is EditSubjectNavigationParams p && p.Subject != null)
            {
                _subject = p.Subject;
                Name = _subject.Name;
                KhmerName = _subject.KhmerName;
                MaxScore = _subject.MaxScore;
                IsActive = _subject.IsActive;
            }

            return Task.CompletedTask;
        }

        public Task LoadAsync() => Task.CompletedTask;

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _messageService.Show("សូមបញ្ចូលឈ្មោះមុខវិជ្ជា!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(KhmerName))
            {
                _messageService.Show("សូមបញ្ចូលឈ្មោះមុខវិជ្ជាជាភាសាខ្មែរ!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (MaxScore <= 0)
            {
                _messageService.Show("សូមបញ្ចូលពិន្ទុអតិបរមាឲ្យបានត្រឹមត្រូវ!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsSaving = true;

            try
            {
                _subject.Name = Name.Trim();
                _subject.KhmerName = KhmerName.Trim();
                _subject.MaxScore = MaxScore;
                _subject.IsActive = IsActive;

                var response = await _subjectService.UpdateAsync(_subject);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានកែប្រែមុខវិជ្ជាដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                    await _navigationService.NavigateAsync<SubjectListViewModel>();
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសក្នុងការរក្សាទុក!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await _navigationService.NavigateAsync<SubjectListViewModel>();
        }
    }
}
