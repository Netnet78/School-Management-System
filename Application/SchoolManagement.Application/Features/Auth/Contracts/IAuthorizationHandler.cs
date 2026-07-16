namespace SchoolManagement.Application.Features.Auth.Contracts
{
    /// <summary>
    /// Defines a contract for evaluating whether a user is authorized to perform actions on a specified resource based
    /// on permission requirements.
    /// </summary>
    /// <remarks>Implementations of this interface determine if a user meets one or more permission
    /// requirements for a given resource. This is typically used in authorization systems to centralize and standardize
    /// permission checks across different resources and operations.</remarks>
    public interface IAuthorizationHandler
    {
        /// <summary>
        /// Evaluates whether a given user satisfies specific permissions requirement for a provided resource.
        /// </summary>
        /// <param name="user">
        /// The current user whose permissions are being checked.
        /// </param>
        /// <param name="resource">
        /// The target resource the user is attempting to access or perform an action on.
        /// This can be any object relevant to the authorization context.
        /// </param>
        /// <param name="operatorMode">
        /// The logical operator for evaluating the result (AND/OR)
        /// </param>
        /// <param name="requirements">
        /// The required permissions that must be fulfilled (e.g., Read, Write, Delete).
        /// </param>
        /// <returns>
        /// A task that resolves to <c>true</c> if the user meets the permissions requirement;
        /// otherwise, <c>false</c>.
        /// </returns>
        Task<bool> HandleAsync(User? user, object? resource, OperatorMode operatorMode, params PermissionType []requirements);

    }
}
