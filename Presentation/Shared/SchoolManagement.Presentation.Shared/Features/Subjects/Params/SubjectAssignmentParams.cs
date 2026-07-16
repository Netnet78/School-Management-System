using SchoolManagement.Core.Features.Classes.Models;

namespace SchoolManagement.Presentation.Shared.Features.Subjects.Params
{
    public class SubjectAssignmentParams : INavigationParams
    {
        public Class? Class { get; set; }
    }
}
