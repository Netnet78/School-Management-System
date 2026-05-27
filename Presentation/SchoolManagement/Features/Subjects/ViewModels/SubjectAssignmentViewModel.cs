using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Presentation.Shared.Features.Subjects.Params;
using SchoolManagement.Presentation.Shared.Features.Subjects.Observables;
using System.Collections.ObjectModel;
using SchoolManagement.Presentation.Features.Classes.ViewModels;

namespace SchoolManagement.Presentation.Features.Subjects.ViewModels
{
    public partial class SubjectAssignmentViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly ISubjectService _subjectService;
        private readonly IClassSubjectService _classSubjectService;
        private readonly IEmployeeService _employeeService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

        private Class? _class;

        [ObservableProperty]
        private Subject? _selectedAvailableSubject;

        [ObservableProperty]
        private SubjectSelectionItem? _selectedSelectedSubject;

        [ObservableProperty]
        private ObservableCollection<Subject> _availableSubjects = [];

        [ObservableProperty]
        private ObservableCollection<SubjectSelectionItem> _selectedSubjects = [];

        [ObservableProperty]
        private ObservableCollection<Employee> _teachers = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _className = string.Empty;

        public SubjectAssignmentViewModel(
            ISubjectService subjectService,
            IClassSubjectService classSubjectService,
            IEmployeeService employeeService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _subjectService = subjectService;
            _classSubjectService = classSubjectService;
            _employeeService = employeeService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is SubjectAssignmentParams p)
            {
                _class = p.Class;
                if (_class != null)
                {
                    ClassName = _class.GetKhmerName();
                }
            }
        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            try
            {
                var teachersResponse = await _employeeService.GetAllAsync(1);
                if (teachersResponse.Status == Status.Success && teachersResponse.Value != null)
                {
                    Teachers = new ObservableCollection<Employee>(teachersResponse.Value);
                }

                var subjectsResponse = await _subjectService.GetAllAsync(
                    filters: [new FilterCondition<Subject>(s => s.IsActive, FilterOperator.Equals, true)]);
                if (subjectsResponse.Status == Status.Success && subjectsResponse.Value != null)
                {
                    List<Subject> allSubjects = subjectsResponse.Value.ToList();

                    if (_class != null)
                    {
                        HashSet<int> selectedSubjectIds = _class.Subjects
                            .Select(cs => cs.SubjectId)
                            .ToHashSet();

                        AvailableSubjects = new ObservableCollection<Subject>(
                            allSubjects.Where(s => !selectedSubjectIds.Contains(s.Id)));

                        SelectedSubjects = new ObservableCollection<SubjectSelectionItem>(
                            _class.Subjects.Select(cs => new SubjectSelectionItem(
                                cs.Subject,
                                Teachers.FirstOrDefault(t => t.Id == cs.TeacherId))));
                    }
                    else
                    {
                        AvailableSubjects = new ObservableCollection<Subject>(allSubjects);
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AddSubject()
        {
            if (SelectedAvailableSubject == null) return;
            Subject item = SelectedAvailableSubject;
            AvailableSubjects.Remove(item);
            SelectedSubjects.Add(new SubjectSelectionItem(item));
            SelectedAvailableSubject = null;
        }

        [RelayCommand]
        private void RemoveSubject()
        {
            if (SelectedSelectedSubject == null) return;
            SubjectSelectionItem item = SelectedSelectedSubject;
            SelectedSubjects.Remove(item);
            AvailableSubjects.Add(item.Subject);
            SelectedSelectedSubject = null;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            SubjectSelectionItem? noTeacherSubject = SelectedSubjects.FirstOrDefault(s => s.SelectedTeacher == null);
            if (noTeacherSubject != null)
            {
                _messageService.Show($"សូមធ្វើការជ្រើសរើសគ្រូម្នាក់​សម្រាប់មុខវិជ្ជា \"{noTeacherSubject.Subject.KhmerName}\" មុននឹងបន្តទៅមុខ"
                    , "ឈប់សិន! ព័ត៌មានមិនគ្រប់គ្រាន់ទេ!", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            if (_class == null)
            {
                _messageService.Show("មិនអាចកំណត់ថ្នាក់បានទេ!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            IsLoading = true;

            try
            {
                var subjects = SelectedSubjects
                    .Select(s => (s.Subject.Id, (int?)s.SelectedTeacher?.Id))
                    .ToList();

                var syncResponse = await _classSubjectService.SyncSubjectsForClassAsync(_class.Id, subjects);
                if (syncResponse.Status != Status.Success)
                {
                    _messageService.Show(syncResponse.Message ?? "មានកំហុសក្នុងការរក្សាទុកមុខវិជ្ជា!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                _messageService.Show("បានរក្សាទុកមុខវិជ្ជាដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                await _navigationService.NavigateAsync<ClassViewModel>();
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
        private async Task CancelAsync()
        {
            await GoBack();
        }

        private async Task GoBack()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;
            if (previous != null)
            {
                await _navigationService.NavigateAsync(previous.GetType());
            }
            else
            {
                await _navigationService.NavigateAsync<ClassViewModel>();
            }
        }
    }
}
