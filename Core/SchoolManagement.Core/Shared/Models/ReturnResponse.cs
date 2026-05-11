using SchoolManagement.Core.Enums;


namespace SchoolManagement.Core.Shared.Models
{
    public class ReturnResponse
    {
        required public Status Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ReturnResponse<T> : ReturnResponse
    {
        public T? Value { get; set; }
    }
}
