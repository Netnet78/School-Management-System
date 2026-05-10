using School_Management.Core.Enums;

namespace School_Management.Core.Attributes
{
    /// <summary>
    /// Indicates that the decorated class should be excluded from auditing operations.
    /// </summary>
    /// <remarks>Apply this attribute to a class to prevent it from being included in audit logs or audit
    /// processing. This is typically used to mark types that contain sensitive information or are not relevant for
    /// auditing purposes.</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class AuditIgnoreTypeAttribute : Attribute
    {
        public AuditOperation Operation { get; }
        public AuditIgnoreTypeAttribute(AuditOperation operation)
        {
            Operation = operation;
        }
    }
}
