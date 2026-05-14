namespace SchoolManagement.Core.Shared.Contracts
{
    /// <summary>
    /// Defines a contract for entities that provide an audit-friendly name for identification in audit logs or tracking
    /// systems.
    /// </summary>
    /// <remarks>Implement this interface to enable consistent retrieval of a human-readable or unique name
    /// for auditing purposes. This name is typically used in audit trails, change tracking, or logging scenarios to
    /// identify the entity involved in an operation.</remarks>
    public interface IAuditableEntity
    {
        string GetAuditName();
    }
}
