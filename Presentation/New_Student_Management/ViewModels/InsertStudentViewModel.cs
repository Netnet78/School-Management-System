using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using School_Management.Core.Models;
using School_Management.Core.Enums;
using School_Management.Infrastructure.Repositories;
using System.Windows;
using School_Management.Presentation.Shared.Helpers;

namespace New_Student_Management.ViewModels
{
    public partial class InsertStudentViewModel : ObservableObject
    {
        private readonly IStudentRepository _repo;

        [ObservableProperty]
        private Candidate data;

        [ObservableProperty]
        private int currentStep = 0;

        [ObservableProperty]
        private string? currentPhotoPath;

        // Expose enum options as value & description
        public IEnumerable<object> GenderOptions
        { get; } = Enum.GetValues<Gender>()
            .Select(g => new { Value = g, Description = g.GetDescription() });

        public IEnumerable<object> StayTypeOptions
        { get; } = Enum.GetValues<StudentStayType>()
            .Select(s => new { Value = s, Description = s.GetDescription() });

        public IEnumerable<object> SkillOptions
        { get; } = Enum.GetValues<StudentSkill>()
            .Select(s => new { Value = s, Description = EnumExtensions.GetDescription(s) });

        public InsertStudentViewModel(IStudentRepository repo)
        {
            _repo = repo;
            Data = new()
            {
                FirstName = string.Empty,
                LatinFirstName = string.Empty,
                LastName = string.Empty,
                LatinLastName = string.Empty,
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                Gender = Gender.Male,
                Skill = StudentSkill.Computer,
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
                        MessageBox.Show("Please enter the student's name!", "Name error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        MessageBox.Show("Please enter the student's name!", "Name error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }

                if (Data.Age < (DateTime.Now.Year - DateTime.Now.AddYears(-12).Year))
                {
                    MessageBox.Show("Please make sure that you enter a valid date of birth!", "Birthday error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                await _repo.AddStudentAsync(Data);

                MessageBox.Show("Successfully added the candidate!", "Insert Success!", MessageBoxButton.OK, MessageBoxImage.Information);

                DateOnly previousExamDate = Data.ExamDate;
                Data = new()
                {
                    FirstName = string.Empty,
                    LatinFirstName = string.Empty,
                    LastName = string.Empty,
                    LatinLastName = string.Empty,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                    Gender = Gender.Male,
                    Skill = StudentSkill.Computer,
                    ExamDate = previousExamDate,
                    PhotoPath = string.Empty,
                };
                CurrentPhotoPath = Data.PhotoPath;

                CurrentStep = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error when trying to add another student.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show(ex.Message);
            }
        }

        [RelayCommand]
        private void UploadPhoto()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg,*.bmp,*.gif",
                Title = "Select a Photo",
                Multiselect = false,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Data.PhotoPath = openFileDialog.FileName;
                CurrentPhotoPath = openFileDialog.FileName;
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
