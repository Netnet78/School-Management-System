namespace SchoolManagement.Core.Shared.Attributes
{
    /// <summary>
    /// Specifies that the associated property should be excluded from auditing operations.
    /// </summary>
    /// <remarks>Apply this attribute to a property to prevent its value from being included in audit logs or
    /// change tracking. This is useful for sensitive or irrelevant data that should not be recorded during auditing
    /// processes.</remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class AuditMaskAttribute : Attribute
    {
    }
}
