using SchoolManagement.Core.Enums;

namespace SchoolManagement.Core.Shared.Models
{
    public class GenderOption
    {
        public Gender? Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
    }
}
