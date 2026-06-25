using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Features.Students.Models;

namespace SchoolManagement.Presentation.Shared.Features.Scores.Observables
{
    public partial class StudentScoreEntry : ObservableObject
    {
        public Student Student { get; }
        public int StudentClassId { get; }

        [ObservableProperty]
        private Assessment? _existingAssessment;

        [ObservableProperty]
        private decimal _scoreAmount;

        public StudentScoreEntry(Student student, int studentClassId, Assessment? existingAssessment = null)
        {
            Student = student;
            StudentClassId = studentClassId;
            ExistingAssessment = existingAssessment;
            ScoreAmount = existingAssessment?.TotalScore ?? 0;
        }
    }
}
