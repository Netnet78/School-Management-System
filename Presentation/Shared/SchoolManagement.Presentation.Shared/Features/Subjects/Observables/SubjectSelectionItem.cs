using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Subjects.Models;

namespace SchoolManagement.Presentation.Shared.Features.Subjects.Observables
{
    public partial class SubjectSelectionItem : ObservableObject
    {
        [ObservableProperty]
        private Subject _subject = null!;

        [ObservableProperty]
        private Employee? _selectedTeacher;

        public SubjectSelectionItem() { }

        public SubjectSelectionItem(Subject subject, Employee? selectedTeacher = null)
        {
            Subject = subject;
            SelectedTeacher = selectedTeacher;
        }
    }
}
