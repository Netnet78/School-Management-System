using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using New_Student_Management.Helpers;
using New_Student_Management.Models;
using New_Student_Management.Services;
using System.Windows;

namespace New_Student_Management.ViewModels
{
    public partial class EditStudentViewModel : ObservableObject
    {
        private readonly IStudentRepository _repo;
        private readonly Student _original;
        public Action<bool>? RequestClose { get; set; }

        [ObservableProperty]
        private int currentStep = 0;

        [ObservableProperty]
        private Student editedStudent;

        [ObservableProperty]
        private string? currentPhotoPath;

        public EditStudentViewModel(Student studentToEdit, IStudentRepository repository)
        {
            _repo = repository;
            _original = studentToEdit ?? throw new ArgumentNullException(nameof(studentToEdit));
            // create an editable copy so we only persist on Save
            EditedStudent = Clone(studentToEdit);
            CurrentPhotoPath = EditedStudent.PhotoPath;
        }

        private static Student Clone(Student s) => s;

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
                    await _repo.AddStudentAsync(EditedStudent);
                }
                else
                {
                    await _repo.UpdateStudentAsync(EditedStudent);
                }
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                RequestClose?.Invoke(false);
                MessageBox.Show("There was an error saving the student data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show(ex.Message);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(false);
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
                EditedStudent.PhotoPath = openFileDialog.FileName;
                CurrentPhotoPath = openFileDialog.FileName;
            }
        }

        // Expose enum options as value & description
        public IEnumerable<object> GenderOptions
        { get;  } = Enum.GetValues<StudentGender>()
            .Cast<StudentGender>()
            .Select(g => new {Value = g, Description = EnumExtensions .GetDescription(g)});

        public IEnumerable<object> StayTypeOptions
        { get; } = Enum.GetValues<StudentStayType>()
            .Cast<StudentStayType>()
            .Select(s => new { Value = s, Description = EnumExtensions.GetDescription(s)});

        public IEnumerable<object> SkillOptions
        { get; } = Enum.GetValues<StudentSkill>()
            .Cast<StudentSkill>()
            .Select(s => new { Value = s, Description = EnumExtensions.GetDescription(s)});
    }
}