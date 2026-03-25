using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using School_Management.Core.Models;
using School_Management.Core.Enums;
using School_Management.Infrastructure.Repositories;
using System.Windows;
using School_Management.Presentation.Shared.Helpers;
using School_Management.Core.Interfaces;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Enums;

namespace New_Student_Management.ViewModels
{
    public partial class EditStudentViewModel : ObservableObject
    {
        private readonly ICandidateRepository _repo;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IPhotoDeleteService _photoDeleteService;
        private readonly IPhotoUploadService _photoUploadService;
        private readonly IMessageService _messageService;
        public Action<bool>? RequestClose { get; set; }

        [ObservableProperty]
        private int currentStep = 0;

        [ObservableProperty]
        private Candidate editedStudent;

        private Candidate _defaultStudent;

        [ObservableProperty]
        private FileObject? currentPhoto;

        public EditStudentViewModel(
            Candidate studentToEdit,
            ICandidateRepository repository,
            IPhotoFetchService photoFetchService,
            IPhotoDeleteService photoDeleteService,
            IPhotoUploadService photoUploadService,
            IMessageService messageService)
        {
            _repo = repository;
            _photoDeleteService = photoDeleteService;
            _photoFetchService = photoFetchService;
            _photoUploadService = photoUploadService;
            _messageService = messageService;

            // create an editable copy so we only persist on Save
            _defaultStudent = studentToEdit.Clone();
            EditedStudent = _defaultStudent.Clone();
            CurrentPhoto = new(EditedStudent.PhotoKey);
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
                    await _repo.AddAsync(EditedStudent);
                }
                else
                {
                    EditedStudent.LatinFirstName = EditedStudent.LatinFirstName.ToUpper();
                    EditedStudent.LatinLastName = EditedStudent.LatinLastName.ToUpper();
                    await _repo.UpdateAsync(EditedStudent);
                }
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                RequestClose?.Invoke(false);
                _messageService.Show("There was an error saving the student data.", "Error", MessageBoxButton.OK, MessageBoxIcon.Error);
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
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg,*.bmp,*.gif",
                Title = "Select a Photo",
                Multiselect = false,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                FileObject uploadedPath = await _photoUploadService.UploadStudentPhoto(openFileDialog.FileName);
                EditedStudent.PhotoKey = uploadedPath.FullFileName;
                CurrentPhoto = new(uploadedPath.FilePath);
            }
        }

        // Expose enum options as value & description
        public IEnumerable<object> GenderOptions
        { get;  } = Enum.GetValues<Gender>()
            .Select(g => new { Value = g, Description = EnumExtensions.GetDescription(g) });

        public IEnumerable<object> StayTypeOptions
        { get; } = Enum.GetValues<StudentStayType>()
            .Select(s => new { Value = s, Description = EnumExtensions.GetDescription(s)});
    }
}