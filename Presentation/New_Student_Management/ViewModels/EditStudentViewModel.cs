using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using School_Management.Presentation.Shared.Services;

namespace New_Student_Management.ViewModels
{
    public partial class EditStudentViewModel : ObservableObject, IAsyncLoadable
    {
        private readonly ICandidateService _candidateService;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IPhotoDeleteService _photoDeleteService;
        private readonly IPhotoUploadService _photoUploadService;
        private readonly IMessageService _messageService;
        private readonly IFileDialogService _fileDialogService;

        public Action<bool>? RequestClose { get; set; }

        [ObservableProperty]
        private int _currentStep = 0;

        [ObservableProperty]
        private Candidate _editedStudent;

        private Candidate _defaultStudent;

        [ObservableProperty]
        private FileObject? currentPhoto;

        public EditStudentViewModel(
            Candidate studentToEdit,
            ICandidateService candidateService,
            IPhotoFetchService photoFetchService,
            IPhotoDeleteService photoDeleteService,
            IPhotoUploadService photoUploadService,
            IMessageService messageService,
            IFileDialogService fileDialogService)
        {
            _candidateService = candidateService;
            _photoDeleteService = photoDeleteService;
            _photoFetchService = photoFetchService;
            _photoUploadService = photoUploadService;
            _messageService = messageService;
            _fileDialogService = fileDialogService;

            _defaultStudent = studentToEdit.Clone();
            EditedStudent = null!;
            CurrentPhoto = null;
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
                if (EditedStudent.Id == 0)
                {
                    await _candidateService.InsertCandidateAsync(EditedStudent);
                }
                else
                {
                    EditedStudent.LatinFirstName = EditedStudent.LatinFirstName.ToUpper();
                    EditedStudent.LatinLastName = EditedStudent.LatinLastName.ToUpper();
                    await _candidateService.EditCandidateAsync(EditedStudent);
                }
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                RequestClose?.Invoke(false);
                _messageService.Show("មិនអាចរក្សាទុកទិន្នន័យសិស្សនេះបានទេ សូមត្រួតពិនិត្យទៅលើ​មូលហេតុខាងក្រោម៖", "មានកំហុសបច្ចេកទេស", MessageButton.OK, MessageIcon.Error);
                _messageService.Show(ex.Message);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            EditedStudent = _defaultStudent.Clone();
            CurrentPhoto = new(EditedStudent.PhotoKey);
            CurrentStep = 0;
            RequestClose?.Invoke(false);
        }

        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            if (CurrentPhoto != null)
            {
                ReturnResponse deleteResponse = await _photoDeleteService.DeleteStudentPhoto(CurrentPhoto.FileKey);

                if (deleteResponse.Status == ReturnStatus.Failed)
                {
                    _messageService.Show($"Failed to delete student photo for some reason...\n{deleteResponse.Message}");
                    return;
                }
            }

            FileObjectDialog fileDialog = _fileDialogService.ShowDialog("Select a Photo", false, "Image Files", "png", "jpg", "jpeg", "bmp", "gif");

            FileObject uploadedPath = await _photoUploadService.UploadStudentPhoto(fileDialog.GetFilePath());
            EditedStudent.PhotoKey = uploadedPath.FileKey;
            CurrentPhoto = new(uploadedPath.FilePath);
        }

        public async Task LoadAsync()
        {
            // create an editable copy so we only persist on Save
            EditedStudent = _defaultStudent.Clone();
            CurrentPhoto = await _photoFetchService.GetStudentPhoto(EditedStudent.PhotoKey);
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