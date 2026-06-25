using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Subjects.Models;

namespace SchoolManagement.Presentation.Features.Subjects.ViewModels
{
    public partial class AddSubjectViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly ISubjectService _subjectService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _khmerName = string.Empty;

        [ObservableProperty]
        private decimal _maxScore = 100;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private bool _isSaving;

        public AddSubjectViewModel(
            ISubjectService subjectService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _subjectService = subjectService;
            _messageService = messageService;
            _navigationService = navigationService;
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
                Subject subject = new()
                {
                    Name = Name.Trim(),
                    KhmerName = KhmerName.Trim(),
                    MaxScore = MaxScore,
                    IsActive = IsActive
                };

                var response = await _subjectService.InsertAsync(subject);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានបន្ថែមមុខវិជ្ជាដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
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
