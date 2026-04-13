namespace School_Management.Core.Models
{
    public class ValidationError
    {
        required public string PropertyName { get; set; }
        required public string ErrorMessage { get; set; }
    }
}
