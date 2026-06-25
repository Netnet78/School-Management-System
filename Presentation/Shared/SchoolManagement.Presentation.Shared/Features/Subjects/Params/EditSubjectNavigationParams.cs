using SchoolManagement.Core.Features.Subjects.Models;

namespace SchoolManagement.Presentation.Shared.Features.Subjects.Params
{
    public class EditSubjectNavigationParams : INavigationParams
    {
        public Subject Subject { get; set; } = null!;
    }
}
