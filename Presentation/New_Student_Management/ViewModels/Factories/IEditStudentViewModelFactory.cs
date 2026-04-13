using Microsoft.Extensions.DependencyInjection;
using School_Management.Core.Models;

namespace New_Student_Management.ViewModels.Factories;
public interface IEditStudentViewModelFactory
{
    EditStudentViewModel Create(Candidate candidate);
}

public class EditStudentViewModelFactory : IEditStudentViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EditStudentViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public EditStudentViewModel Create(Candidate candidate)
    {
        return ActivatorUtilities.CreateInstance<EditStudentViewModel>(
            _serviceProvider, 
            candidate);
    }
}