using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Features.Assessments.Models;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Shared.Features.Scores.Observables
{
    public partial class StudentScoreEntry : ObservableObject
    {
        public Student Student { get; }
        public int StudentClassId { get; }

        [ObservableProperty]
        private Assessment? _existingAssessment;

        public ObservableCollection<ComponentScoreEntry> ComponentScores { get; } = [];

        public StudentScoreEntry(Student student, int studentClassId, Assessment? existingAssessment = null)
        {
            Student = student;
            StudentClassId = studentClassId;
            ExistingAssessment = existingAssessment;
        }
    }
}
