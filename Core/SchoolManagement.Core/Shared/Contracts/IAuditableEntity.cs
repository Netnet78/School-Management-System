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
        /// <summary>
        /// Gets the custom audit name used for audit logging.
        /// </summary>
        /// <returns>A string representing the custom audit name.</returns>
        string CustomAuditName();
        /// <summary>
        /// Gets a custom audit description for the current context.
        /// </summary>
        /// <returns>A string that represents the custom audit description.</returns>
        string CustomAuditDescription();
    }
}
