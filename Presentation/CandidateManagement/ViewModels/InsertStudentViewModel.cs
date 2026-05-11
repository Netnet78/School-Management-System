using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Presentation.Contracts;
using SchoolManagement.Core.Shared.Extensions;
using SchoolManagement.Core.Shared.Models;

namespace CandidateManagement.ViewModels
{
    public partial class InsertStudentViewModel : ObservableObject, IAsyncLoadable, IViewModel
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IPhotoUploadService _photoUploadService;
        private readonly IMessageService _messageService;
        private readonly ISoundService _soundService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDispatcherService _dispatcherService;
        private readonly SoundObject _errorSound = new("Sources\\Audio\\sfx\\error-sound.wav");

        [ObservableProperty]
        private Candidate data;

        [ObservableProperty]
        private int currentStep = 0;

        [ObservableProperty]
        private FileObject? currentPhoto;

        [ObservableProperty]
        private IEnumerable<Skill> skillOptions = [];

        // Expose enum options as value & description
        public IEnumerable<object> GenderOptions
        { get; } = Enum.GetValues<Gender>()
            .Select(g => new { Value = g, Description = g.GetDescription() });

        public IEnumerable<object> StayTypeOptions
        { get; } = Enum.GetValues<StudentStayType>()
            .Select(s => new { Value = s, Description = s.GetDescription() });


        public InsertStudentViewModel(
            ICandidateRepository repo,
            ISkillRepository skillRepository,
            IPhotoUploadService photoUploadService,
            IMessageService messageService,
            ISoundService soundService,
            IFileDialogService fileDialogService,
            IDispatcherService dispatcherService)
        {
            _candidateRepository = repo;
            _skillRepository = skillRepository;
            _photoUploadService = photoUploadService;
            _messageService = messageService;
            _soundService = soundService;
            _fileDialogService = fileDialogService;
            _dispatcherService = dispatcherService;

            Data = new()
            {
                FirstName = string.Empty,
                LatinFirstName = string.Empty,
                LastName = string.Empty,
                LatinLastName = string.Empty,
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                Gender = Gender.Male,
                SkillId = 0,
                Photo = null,
            };
        }

        public async Task LoadAsync()
        {
            SkillOptions = await _skillRepository.GetAllAsync();

            _soundService.Load(_errorSound);

            if (Data.SkillId == 0)
            {
                Skill? firstSkill = SkillOptions.FirstOrDefault();
                if (firstSkill != null)
                {
                    Data.SkillId = firstSkill.Id;
                }
            }
        }

        [RelayCommand]
        private async Task AddStudentAsync()
        {
            if (Data == null) return;

            try
            {
                Data.LatinFirstName = Data.LatinFirstName.ToUpper();
                Data.LatinLastName = Data.LatinLastName.ToUpper();

                string[] requiredFields = [Data.FirstName, Data.LastName];

                foreach (string value in requiredFields)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        await TriggerError("Please enter the student's name!", "Name error");
                        return;
                    }
                }

                if (Data.Age < 12)
                {
                    await TriggerError("Please make sure that you enter a valid date of birth!", "Birthday error");
                    return;
                }

                await _candidateRepository.AddAsync(Data);

                // Upload photo after add a new student
                if (CurrentPhoto != null)
                {
                    await _photoUploadService.UploadStudentPhoto(CurrentPhoto.FileKey, Data);
                }

                await _dispatcherService.InvokeAsync(() =>
                {
                    _messageService.Show("Successfully added the candidate!", "Insert Success!", MessageButton.OK, MessageIcon.Information);
                });

                DateOnly? previousExamDate = Data.ExamDate;
                Data = new()
                {
                    FirstName = string.Empty,
                    LatinFirstName = string.Empty,
                    LastName = string.Empty,
                    LatinLastName = string.Empty,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                    Gender = Gender.Male,
                    ExamDate = previousExamDate,
                    Photo = null
                };
                CurrentPhoto = null;
                CurrentStep = 0;
            }
            catch (Exception ex)
            {
                _ = Task.Run(() => _soundService.Play(_errorSound));
                await _dispatcherService.InvokeAsync(async () =>
                {
                    _messageService.Show("There was an error when trying to add another student.", "ERROR", MessageButton.OK, MessageIcon.Error);
                    _messageService.Show(ex.Message);
                });
            }
        }

        private async Task TriggerError(string error, string errorTitle)
        {
            _= Task.Run(() => _soundService.Play(_errorSound));
            await _dispatcherService.InvokeAsync(() => _messageService.Show(error, errorTitle, MessageButton.OK, MessageIcon.Error));
        }

        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            FileDialogObject fileDialog = _fileDialogService
                .ShowDialog("Select a Photo", false,
                "Image Files", "png", "jpg", "jpeg", "bmp", "gif");

            if (fileDialog.File != null)
            {
                string selectedPath = fileDialog.File.FileName;
                FileObject uploadedFile = new(selectedPath);
                
                if (Data.Photo != null)
                {
                    Data.Photo.Key = uploadedFile.FileKey;
                }
                else
                {
                    Data.Photo = new()
                    {
                        Key = uploadedFile.FileKey
                    };
                }

                CurrentPhoto = uploadedFile;
            }
        }

        [RelayCommand]
        private void Next()
        {
            if (CurrentStep < 3) CurrentStep++;
        }

        [RelayCommand]
        private void Back()
        {
            if (CurrentStep > 0) CurrentStep--;
        }
    }
}
