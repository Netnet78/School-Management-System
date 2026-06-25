namespace SchoolManagement.Core.Shared.Attributes
{
    /// <summary>
    /// Indicates the property should be ignored when it updates
    /// </summary>
    /// <remarks>Apply this attribute to a property to prevent it from being tracked by the interceptor</remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class AuditIgnoreAttribute : Attribute
    {
        
    }
}
