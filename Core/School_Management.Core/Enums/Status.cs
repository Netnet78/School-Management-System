namespace School_Management.Core.Enums
{
    /// <summary>
    /// Represents the result or current state of an operation.
    /// </summary>
    /// <remarks>
    /// This enum is typically used as a standardized response indicator
    /// for service methods, validation results, or business operations.
    /// </remarks>
    public enum Status
    {
        /// <summary>
        /// The operation completed successfully without any issues.
        /// </summary>
        Success,

        /// <summary>
        /// The operation is still in progress or awaiting further action.
        /// </summary>
        Pending,

        /// <summary>
        /// The operation failed due to an error or unexpected condition.
        /// </summary>
        Failed,

        /// <summary>
        /// The operation was explicitly rejected based on validation rules
        /// or business logic.
        /// </summary>
        Rejected,
        
        /// <summary>
        /// The operation doesn't return anything. Simply put, the resource that the operation is trying
        /// to find doesn't exist.
        /// </summary>
        NotFound
    }
}
