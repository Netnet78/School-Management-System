using School_Management.Core.Enums;


namespace School_Management.Core.Models
{
    public class ReturnResponse
    {
        required public ReturnStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ReturnResponse<T> : ReturnResponse
    {
        public T? Value { get; set; }
    }
}
