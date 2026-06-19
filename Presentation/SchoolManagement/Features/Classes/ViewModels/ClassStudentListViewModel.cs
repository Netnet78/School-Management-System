using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Presentation.Features.Scores.ViewModels;
using SchoolManagement.Presentation.Features.Students.ViewModels;
using SchoolManagement.Presentation.Shared.Features.Scores.Params;
using SchoolManagement.Presentation.Shared.Features.Students.Params;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Classes.ViewModels
{
    public partial class ClassStudentListViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IStudentClassService _studentClassService;
        private readonly INavigationService _navigationService;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private Class _class = null!;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _classInfo = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Student> _students = [];

        [ObservableProperty]
        private bool _isLoadingStudents;

        [ObservableProperty]
        private bool _canEditStudents;

        public ClassStudentListViewModel(
            IStudentClassService studentClassService,
            INavigationService navigationService,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _studentClassService = studentClassService;
            _navigationService = navigationService;
            _messageService = messageService;
            _authorizationService = authorizationService;
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is not ClassStudentListParams p || p.Class == null)
            {
                _messageService.Show("មិនអាចរកឃើញទិន្នន័យថ្នាក់!", "មានកំហុស!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            _class = p.Class;
            ClassName = _class.GetKhmerName();
            ClassInfo = $"ថ្នាក់: {_class.GetKhmerName()} | កម្រិត: {_class.Grade?.KhmerName} | ជំនាន់: {_class.Generation?.CohortNumber} | ផ្នែក: {_class.Generation?.Department?.KhmerName}";
        }

        public async Task LoadAsync()
        {
            if (_class == null) return;

            User? user = _authorizationService.CurrentUser;
            if (user == null)
            {
                _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }
            CanEditStudents = (await _authorizationService.AuthorizeAsync(null, PermissionType.EditStudents)).Status == Status.Success;

            await LoadStudentsAsync();
        }

        private async Task LoadStudentsAsync()
        {
            IsLoadingStudents = true;

            try
            {
                var scResponse = await _studentClassService.GetAllAsync(
                    filters: [new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, _class.Id)],
                    page: 1,
                    includes: ["Student.Candidate", "Student.Candidate.Skill", "Student.Candidate.Photo"]);

                Students.Clear();

                if (scResponse.Status == Status.Success && scResponse.Value != null)
                {
                    foreach (StudentClass sc in scResponse.Value)
                    {
                        if (sc.Student != null)
                        {
                            Students.Add(sc.Student);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យសិស្ស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoadingStudents = false;
            }
        }

        [RelayCommand]
        private async Task EditStudentAsync(Student? student)
        {
            if (student == null) return;

            if (!CanEditStudents)
            {
                _messageService.Show("អ្នកគ្មានសិទ្ធិកែប្រែសិស្សទេ!", "គ្មានសិទ្ធិ!", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            await _navigationService.NavigateAsync<EditStudentViewModel>(new EditStudentNavigationParams { Student = student });
        }

        [RelayCommand]
        private async Task ShowScoresAsync()
        {
            if (_class == null) return;

            await _navigationService.NavigateAsync<ScoreViewModel>(new ScoreNavigationParams { Class = _class });
        }

        [RelayCommand]
        private async Task ShowAddStudentsToClass()
        {
            await _navigationService.NavigateAsync<AddStudentsToClassViewModel>(new AddStudentsToClassParams { Class = _class });
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await _navigationService.NavigateAsync<ClassViewModel>();
        }

    }
}
