using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Core.Features.Files.Models;
using SchoolManagement.Core.Features.Skills.Models;
using SchoolManagement.Presentation.Shared.Features.Classes.Observables;
using SchoolManagement.Presentation.Shared.Features.Students.Observables;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public abstract partial class StudentFormViewModelBase : ObservableObject, IViewModel, IAsyncLoadable
    {
        protected readonly IAuthorizationService _authorizationService;
        protected readonly IMessageService _messageService;
        protected readonly INavigationService _navigationService;
        protected readonly IPhotoUploadService _photoUploadService;
        protected readonly IPhotoDeleteService _photoDeleteService;
        protected readonly IPhotoFetchService _photoFetchService;
        protected readonly ISkillService _skillService;
        protected readonly IClassService _classService;
        protected readonly IStudentClassService _studentClassService;
        protected readonly IStudentService _studentService;
        protected readonly IFileDialogService _fileDialogService;

        private bool _photoChanged;
        private string? _existingPhotoKey;

        [ObservableProperty]
        private StudentForm _studentForm = new();

        [ObservableProperty]
        private string? _currentPhoto;

        [ObservableProperty]
        private bool _canSetSkill;

        [ObservableProperty]
        private bool _canAssignClass;

        [ObservableProperty]
        private ObservableCollection<Skill> _skills = [];

        [ObservableProperty]
        private ObservableCollection<ClassCheckItem> _availableClasses = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canEdit;

        protected StudentFormViewModelBase(
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService,
            IPhotoUploadService photoUploadService,
            IPhotoDeleteService photoDeleteService,
            IPhotoFetchService photoFetchService,
            ISkillService skillService,
            IClassService classService,
            IStudentClassService studentClassService,
            IStudentService studentService,
            IFileDialogService fileDialogService)
        {
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
            _photoUploadService = photoUploadService;
            _photoDeleteService = photoDeleteService;
            _photoFetchService = photoFetchService;
            _skillService = skillService;
            _classService = classService;
            _studentClassService = studentClassService;
            _studentService = studentService;
            _fileDialogService = fileDialogService;

        }

        public abstract Task LoadAsync();

        [RelayCommand]
        protected async Task UploadPhotoAsync()
        {
            FileDialogObject file = _fileDialogService.ShowDialog("Select a Photo", false, "Images files",
                "png", "jpg", "jpeg", "bmp", "gif");

            if (file.File != null)
            {
                CurrentPhoto = file.GetFilePath();
                _photoChanged = true;
            }
        }

        [RelayCommand]
        protected Task DeletePhotoAsync()
        {
            CurrentPhoto = null;
            _photoChanged = true;
            return Task.CompletedTask;
        }

        protected async Task LoadCommonAuthorizationAsync(PermissionType permission)
        {
            var permissionResult = await _authorizationService.AuthorizeAsync(null, permission);

            if (permissionResult.Status != Status.Success)
            {
                CanEdit = false;
                _messageService.Show(
                    "អ្នកមិនមានសិទ្ធិក្នុងការធ្វើប្រតិបត្តិការនេះទេ!",
                    "គ្មានសិទ្ធិ!",
                    MessageButton.OK,
                    MessageIcon.Hand);
                return;
            }

            CanEdit = true;
        }

        protected async Task LoadSkillsAsync()
        {
            var skillsResponse = await _skillService.GetAllAsync(1, null, null, null);

            if (skillsResponse.Status == Status.Success && skillsResponse.Value != null)
            {
                Skills.Clear();
                foreach (Skill skill in skillsResponse.Value)
                {
                    Skills.Add(skill);
                }
            }
        }

        protected async Task LoadAvailableClassesAsync(int? departmentId)
        {
            if (!departmentId.HasValue)
            {
                return;
            }

            var classResponse = await _classService.GetAllAsync(
                filters: [new FilterCondition<Class>(c => c.Generation.DepartmentId, FilterOperator.Equals, departmentId.Value)]);

            if (classResponse.Status == Status.Success && classResponse.Value != null)
            {
                AvailableClasses.Clear();
                foreach (Class cls in classResponse.Value)
                {
                    AvailableClasses.Add(new ClassCheckItem { Class = cls, IsChecked = false });
                }
            }
        }

        protected async Task<HashSet<int>> LoadExistingEnrollmentsAsync(int studentId)
        {
            var enrollmentResponse = await _studentClassService.GetAllAsync(
                filters: [new FilterCondition<StudentClass>(sc => sc.StudentId, FilterOperator.Equals, studentId)]);

            if (enrollmentResponse.Status == Status.Success && enrollmentResponse.Value != null)
            {
                return enrollmentResponse.Value.Select(sc => sc.ClassId).ToHashSet();
            }

            return [];
        }

        protected async Task UploadStudentPhotoAsync(Student student)
        {
            if (!_photoChanged || string.IsNullOrWhiteSpace(CurrentPhoto))
            {
                return;
            }

            ReturnResponse<FileObject> uploadResponse = await _photoUploadService.UploadStudentPhoto(CurrentPhoto, student.Candidate);

            if (uploadResponse.Status == Status.Failed)
            {
                _messageService.Show(
                    uploadResponse.Message ?? "The student photo could not be uploaded.",
                    "Photo upload warning",
                    MessageButton.OK,
                    MessageIcon.Exclamation);
            }

            _photoChanged = false;
        }

        protected async Task DeleteStudentPhotoAsync(Candidate candidate)
        {
            if (!_photoChanged)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_existingPhotoKey) || CurrentPhoto != null)
            {
                return;
            }

            await _photoDeleteService.DeleteStudentPhoto(candidate);
            _existingPhotoKey = null;
            _photoChanged = false;
        }

        protected async Task GoBackAsync()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;

            if (previous != null)
            {
                await _navigationService.NavigateAsync(previous.GetType());
            }
            else
            {
                await _navigationService.NavigateAsync<StudentListViewModel>();
            }
        }

        protected void LoadStudentToForm(Student student)
        {
            StudentForm = new StudentForm(student);
            _existingPhotoKey = student.PhotoKey;
            _photoChanged = false;
        }
    }
}
