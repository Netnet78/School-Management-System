using New_Student_Management.ViewModels;
using New_Student_Management.ViewModels.Factories;
using School_Management.Core.Models;

namespace New_Student_Management.Views.Wizards.Services
{
    public interface IEditStudentWizardService
    {
        public bool? Show(Candidate candidate);
    }

    public class EditStudentWizardService : IEditStudentWizardService
    {
        private readonly IEditStudentViewModelFactory _factory;

        public EditStudentWizardService(IEditStudentViewModelFactory factory)
        {
            _factory = factory;
        }

        public bool? Show(Candidate candidate)
        {
            EditStudentWizard wizard = new(_factory.Create(candidate))
            {
                Owner = App.Current.MainWindow,
            };

            return wizard.ShowDialog();
        }
    }
}
