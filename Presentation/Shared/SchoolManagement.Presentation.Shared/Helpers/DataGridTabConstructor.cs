using System.ComponentModel;

namespace SchoolManagement.Presentation.Shared.Helpers
{
    public class DataGridTabConstructor
    {
        public required string Header { get; set; }
        public required ICollectionView Data { get; init; }
    }
}
