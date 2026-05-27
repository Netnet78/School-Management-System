using SchoolManagement.Core.Features.Classes.Models;

namespace SchoolManagement.Presentation.Shared.Features.Scores.Params
{
    public class ScoreNavigationParams : INavigationParams
    {
        public Class? Class { get; set; }
    }
}
