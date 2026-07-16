using SchoolManagement.Core.Features.Classes.Models;

namespace SchoolManagement.Presentation.Shared.Features.Classes.Params
{
    public class EditClassParams : INavigationParams
    {
        public Class? Class { get; set; }
    }
}
