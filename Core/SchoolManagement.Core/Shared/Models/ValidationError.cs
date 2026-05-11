namespace SchoolManagement.Core.Shared.Models
{
    public class ValidationError
    {
        required public string PropertyName { get; set; }
        required public string ErrorMessage { get; set; }
    }
}
