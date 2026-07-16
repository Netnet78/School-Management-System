using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Presentation.Shared.Features.Subjects.Observables;
using SchoolManagement.Presentation.Shared.Features.Subjects.Params;

namespace SchoolManagement.Presentation.Features.Subjects.ViewModels
{
    public partial class EditSubjectViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly ISubjectService _subjectService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

        private int _subjectId;
        private Subject? _subject;

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

        [ObservableProperty]
        private ObservableCollection<ComponentEntry> _components = [];

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
                _subjectId = p.Subject.Id;
            }

            return Task.CompletedTask;
        }

        public async Task LoadAsync()
        {
            if (_subjectId <= 0) return;

            var response = await _subjectService.GetByIdAsync(_subjectId);
            if (response.Status != Status.Success || response.Value == null)
            {
                _messageService.Show("មិនអាចទាញទិន្នន័យមុខវិជ្ជាបានទេ!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            _subject = response.Value;
            Name = _subject.Name;
            KhmerName = _subject.KhmerName;
            MaxScore = _subject.MaxScore;
            IsActive = _subject.IsActive;

            Components.Clear();
            var mappers = await _subjectService.GetMappersForSubjectAsync(_subjectId);
            foreach (SubjectMapper m in mappers)
            {
                Components.Add(new ComponentEntry(m.Component?.Name ?? string.Empty, m.Component?.KhmerName ?? string.Empty));
            }
        }

        [RelayCommand]
        private void AddComponent()
        {
            Components.Add(new ComponentEntry());
        }

        [RelayCommand]
        private void RemoveComponent(ComponentEntry? entry)
        {
            if (entry != null)
                Components.Remove(entry);
        }

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
                var mappers = Components
                    .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                    .Select(c => new SubjectMapper
                    {
                        Component = new SubjectComponent
                        {
                            Name = c.Name.Trim(),
                            KhmerName = c.KhmerName.Trim()
                        }
                    })
                    .ToList();

                Subject subject = new()
                {
                    Id = _subjectId,
                    Name = Name.Trim(),
                    KhmerName = KhmerName.Trim(),
                    MaxScore = MaxScore,
                    IsActive = IsActive,
                    Mappers = mappers
                };

                var response = await _subjectService.UpdateAsync(subject);

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
