using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    public class GenderOption
    {
        public Gender? Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
    }
}
