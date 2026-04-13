using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using System.Media;
using System.Windows;

namespace New_Student_Management.ViewModels
{
    public partial class InsertStudentViewModel : ObservableObject
    {
        private readonly ICandidateRepository _repo;
        private readonly IPhotoUploadService _photoUploadService;
        private readonly IMessageService _messageService;
        private readonly SoundPlayer errorPlayer = new("Sources\\Audio\\sfx\\error-sound.wav");

        [ObservableProperty]
        private Candidate data;

        [ObservableProperty]
        private int currentStep = 0;

        [ObservableProperty]
        private FileObject? currentPhoto;

        // Expose enum options as value & description
        public IEnumerable<object> GenderOptions
        { get; } = Enum.GetValues<Gender>()
            .Select(g => new { Value = g, Description = g.GetDescription() });

        public IEnumerable<object> StayTypeOptions
        { get; } = Enum.GetValues<StudentStayType>()
            .Select(s => new { Value = s, Description = s.GetDescription() });


        public InsertStudentViewModel(ICandidateRepository repo, IPhotoUploadService photoUploadService, IMessageService messageService)
        {
            _repo = repo;
            _photoUploadService = photoUploadService;
            _messageService = messageService;

            Data = new()
            {
                FirstName = string.Empty,
                LatinFirstName = string.Empty,
                LastName = string.Empty,
                LatinLastName = string.Empty,
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                Gender = Gender.Male,
            };
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
                    if (value == null)
                    {
                        errorPlayer.Play();
                        _messageService.Show("Please enter the student's name!", "Name error", MessageButton.OK, MessageIcon.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        errorPlayer.Play();
                        _messageService.Show("Please enter the student's name!", "Name error", MessageButton.OK, MessageIcon.Error);
                        return;
                    }
                }

                if (Data.Age < (DateTime.Now.Year - DateTime.Now.AddYears(-12).Year))
                {
                    errorPlayer.Play();
                    _messageService.Show("Please make sure that you enter a valid date of birth!", "Birthday error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                await _repo.AddAsync(Data);

                // Upload photo after add a new student
                if (CurrentPhoto != null)
                {
                    await _photoUploadService.UploadStudentPhoto(CurrentPhoto.FullFileName);
                }

                _messageService.Show("Successfully added the candidate!", "Insert Success!", MessageButton.OK, MessageIcon.Information);

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
                    PhotoKey = string.Empty,
                };
                CurrentPhoto = null;
                CurrentStep = 0;
            }
            catch (Exception ex)
            {
                errorPlayer.Play();
                _messageService.Show("There was an error when trying to add another student.", "ERROR", MessageButton.OK, MessageIcon.Error);
                _messageService.Show(ex.Message);
            }
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
                string selectedPath = openFileDialog.FileName;
                FileObject uploadedFile = new(selectedPath);
                Data.PhotoKey = uploadedFile.FullFileName;
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
