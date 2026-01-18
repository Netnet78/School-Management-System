
namespace School_Management.Presentation.Shared.Helpers
{
    public class DataGridTabConstructor<T>
    {

        public required string Header { get; set; }
        public required IEnumerable<T> Data { get; init; }
    }
}
