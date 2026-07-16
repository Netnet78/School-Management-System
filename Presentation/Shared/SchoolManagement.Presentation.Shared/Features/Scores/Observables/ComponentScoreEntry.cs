using CommunityToolkit.Mvvm.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Features.Scores.Observables
{
    public partial class ComponentScoreEntry : ObservableObject
    {
        public int MapperId { get; }
        public int ComponentId { get; }
        public string ComponentName { get; }
        public string ComponentKhmerName { get; }

        [ObservableProperty]
        private decimal _scoreAmount;

        public ComponentScoreEntry(
            int mapperId,
            int componentId,
            string componentName,
            string componentKhmerName,
            decimal scoreAmount = 0)
        {
            MapperId = mapperId;
            ComponentId = componentId;
            ComponentName = componentName;
            ComponentKhmerName = componentKhmerName;
            ScoreAmount = scoreAmount;
        }
    }
}