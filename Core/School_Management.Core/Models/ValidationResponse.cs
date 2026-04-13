namespace School_Management.Core.Models
{
    public class ValidationResponse
    {
        public required bool IsValid { get; set; }
        public ValidationError[] MissingProperties { get; set; } = [];
    }
}
