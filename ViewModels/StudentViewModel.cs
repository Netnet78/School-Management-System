using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Student_Management.Models;
using Student_Management.Services;
using System.Collections.ObjectModel;

namespace Student_Management.ViewModels
{
    public partial class StudentViewModel : ObservableObject
    {
        private readonly IStudentRepository _repository;
        public StudentViewModel(IStudentRepository repository)
        {
            _repository = repository;
            LoadStudentsAsync().ConfigureAwait(false);
        }

        // MVVM Bindings
        public ObservableCollection<Student> Students { get; } = [];

        private Student? _selectedStudent;
        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                SetProperty(ref _selectedStudent, value);
            }
        }

        // Relay Commands
        [RelayCommand]
        private async Task LoadStudentsAsync()
        {
            Students.Clear();
            List<Student> students = await _repository.GetAllStudentsAsync();
            foreach (var student in students)
            {
                Students.Add(student);
            }
        }
        [RelayCommand]
        private async Task EditStudentAsync(Student student)
        {
            if (SelectedStudent == null) return;

            Student previous = SelectedStudent;
            var vm = new EditStudentViewModel(SelectedStudent, _repository);
            var wizard = new Views.Wizards.EditStudentWizard(vm)
            {
                Owner = App.Current.MainWindow,
            };
            bool? result = wizard.ShowDialog();
            if (result == true)
            {
                await LoadStudentsAsync();
                SelectedStudent = previous;
            }
        }
        [RelayCommand]
        private async Task DeleteStudentAsync(Student student)
        {
            await _repository.DeleteStudentAsync(student);
            await LoadStudentsAsync();
        }
        [RelayCommand]
        private async Task SaveStudentAsync()
        {
            await _repository.SaveStudentAsync();
        }

    }
}
