using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using System.IO;

namespace New_Student_Management.ViewModels
{
    public class EditStudentParams : INavigationParams
    {
        public Candidate? Candidate { get; set; }
    }
    public partial class EditStudentViewModel : ObservableObject, IViewModel, INavigationAware
    {
        private readonly ICandidateService _candidateService;
        private readonly ISkillRepository _skillRepository;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IPhotoDeleteService _photoDeleteService;
        private readonly IPhotoUploadService _photoUploadService;
        private readonly IMessageService _messageService;
        private readonly IFileDialogService _fileDialogService;
        private readonly INavigationService _navigationService;

        public Action<bool>? RequestClose { get; set; }

        [ObservableProperty]
        private int _currentStep = 0;

        [ObservableProperty]
        private Candidate _editedStudent;

        private Candidate _defaultStudent;
        private string? _studentPhotoBackup;

        [ObservableProperty]
        private FileObject? currentPhoto;

        [ObservableProperty]
        private IEnumerable<Skill> skillOptions = [];

        public EditStudentViewModel(
            ICandidateService candidateService,
            ISkillRepository skillRepository,
            IPhotoFetchService photoFetchService,
            IPhotoDeleteService photoDeleteService,
            IPhotoUploadService photoUploadService,
            IMessageService messageService,
            IFileDialogService fileDialogService,
            INavigationService navigationService)
        {
            _candidateService = candidateService;
            _skillRepository = skillRepository;
            _photoDeleteService = photoDeleteService;
            _photoFetchService = photoFetchService;
            _photoUploadService = photoUploadService;
            _messageService = messageService;
            _fileDialogService = fileDialogService;
            _navigationService = navigationService;

            EditedStudent = CreateEmptyCandidate();
            _defaultStudent = EditedStudent.Clone();
            CurrentPhoto = null;
        }

        private static Candidate CreateEmptyCandidate()
        {
            return new Candidate
            {
                FirstName = string.Empty,
                LatinFirstName = string.Empty,
                LastName = string.Empty,
                LatinLastName = string.Empty,
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                Gender = Gender.Male,
                SkillId = 0,
                Skill = new Skill(),
                PhotoKey = string.Empty,
            };
        }

        [RelayCommand]
        private void Next()
        {
            if (CurrentStep < 4) CurrentStep++;
        }

        [RelayCommand]
        private void Back()
        {
            if (CurrentStep > 0) CurrentStep--;
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (_studentPhotoBackup != null)
                {
                    FileObject newPhoto = await _photoUploadService.UploadStudentPhoto(_studentPhotoBackup);

                    // delete old photo AFTER success
                    if (!string.IsNullOrEmpty(EditedStudent.PhotoKey))
                    {
                        await _photoDeleteService.DeleteStudentPhoto(EditedStudent.PhotoKey);
                    }

                    EditedStudent.PhotoKey = newPhoto.FileKey;
                }

                await _candidateService.EditCandidateAsync(EditedStudent);

                _studentPhotoBackup = null;

                await GoToPreviousView();
            }
            catch (Exception ex)
            {
                // RequestClose?.Invoke(false);
                _messageService.Show("មិនអាចរក្សាទុកទិន្នន័យសិស្សនេះបានទេ សូមត្រួតពិនិត្យទៅលើ​មូលហេតុខាងក្រោម៖", "មានកំហុសបច្ចេកទេស", MessageButton.OK, MessageIcon.Error);
                _messageService.Show(ex.Message);
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            MessageResult cancelConfirm = _messageService
                .Show("តើអ្នកប្រាកដដែរឬទេថា នឹងមិនរក្សាទុកនូវអ្វីដែលបានផ្លាស់ប្ដូរនៃទិន្នន័យនេះ?",
                "ឈប់សិន!", MessageButton.YesNo, MessageIcon.Question);

            if (cancelConfirm != MessageResult.Yes)
                return;

            EditedStudent = _defaultStudent.Clone();
            CurrentPhoto = await _photoFetchService.GetStudentPhoto(_defaultStudent.PhotoKey);

            _studentPhotoBackup = null;

            await GoToPreviousView();
        }

        private async Task GoToPreviousView()
        {
            Type? prev = _navigationService.PreviousViewModel?.GetType();
            await _navigationService.NavigateAsync(prev ?? typeof(StudentViewModel));
        }

        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            FileDialogObject fileDialog = _fileDialogService.ShowDialog("Select a Photo", false, "Image Files", "png", "jpg", "jpeg", "bmp", "gif");

            if (fileDialog.File == null)
            {
                return;
            }
            _studentPhotoBackup = fileDialog.GetFilePath();

            CurrentPhoto = new FileObject(_studentPhotoBackup);
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is EditStudentParams param && param.Candidate != null)
            {
                SkillOptions = await _skillRepository.GetAllAsync();

                EditedStudent = param.Candidate.Clone();
                _defaultStudent = EditedStudent.Clone();

                CurrentPhoto = await _photoFetchService.GetStudentPhoto(param.Candidate.PhotoKey);
            }
        }

        // Expose enum options as value & description
        public IEnumerable<object> GenderOptions
        { get; } = Enum.GetValues<Gender>()
            .Select(g => new { Value = g, Description = EnumExtensions.GetDescription(g) });

        public IEnumerable<object> StayTypeOptions
        { get; } = Enum.GetValues<StudentStayType>()
            .Select(s => new { Value = s, Description = EnumExtensions.GetDescription(s) });
    }
}
