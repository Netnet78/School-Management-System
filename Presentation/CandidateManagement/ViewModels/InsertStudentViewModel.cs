using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Assets;
using System.IO;

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
        private readonly SoundObject _errorSound = new(Path.Combine(ResourcePaths.Audio, "sfx/error-sound.wav"));

        [ObservableProperty]
        private Candidate data;

        [ObservableProperty]
        private int currentStep = 0;

        [ObservableProperty]
        private string? currentPhoto;

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
            SkillOptions = await _skillRepository.FindAsync(
                filters: [new(s => s.IsActive)]);

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
                        await TriggerError("សូមមេត្តាកុំភ្លេចបំពេញឈ្មោះ សិស្សប្អូន!", "កុំលឿនពេក!!");
                        return;
                    }
                }

                if (Data.Age < 12)
                {
                    await TriggerError("សូមប្អូនត្រួតពិនិត្យមើលឱ្យបានច្បាស់ទាក់ទងនឹងព័ត៌មានថ្ងៃខែឆ្នាំកំណើត", "Birthday error");
                    return;
                }

                await _candidateRepository.AddAsync(Data);

                // Upload photo after add a new student
                if (CurrentPhoto != null)
                {
                    ReturnResponse<FileObject> uploadResponse = await _photoUploadService.UploadStudentPhoto(CurrentPhoto, Data);

                    if (uploadResponse.Status == Status.Failed)
                    {
                        await _dispatcherService.InvokeAsync(() =>
                        {
                            _messageService.Show(uploadResponse.Message, "Photo Upload Warning", MessageButton.OK, MessageIcon.Hand);
                        });
                    }
                }

                await _dispatcherService.InvokeAsync(() =>
                {
                    _messageService.Show($"បេក្ខជនឈ្មោះ {Data.FullName} ត្រូវបានបញ្ចូលជាការស្រេច", "Success!", MessageButton.OK, MessageIcon.Information);
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
                string selectedPath = fileDialog.File.FilePath;
                FileObject uploadedFile = new(selectedPath);

                CurrentPhoto = uploadedFile.FilePath;
            }
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
    }
}
