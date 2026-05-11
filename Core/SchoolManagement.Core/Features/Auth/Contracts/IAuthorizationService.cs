using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Application.Interfaces
{
    /// <summary>
    /// Defines a service for authorizing access to resources based on specified permissions and operator modes.
    /// </summary>
    /// <remarks>Implementations of this interface evaluate whether the current user or context is authorized
    /// to perform actions on a given resource. Authorization decisions are typically based on the provided permissions
    /// and, optionally, the operator mode. This interface is intended to be used in security-sensitive scenarios where
    /// resource access must be controlled.</remarks>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Asynchronously determines whether the specified permissions are granted for the given resource and operator
        /// mode.
        /// </summary>
        /// <param name="resource">The resource for which access is being authorized. Cannot be null.</param>
        /// <param name="operatorMode">The mode in which the operator is acting. Determines the context for permission evaluation.</param>
        /// <param name="permissions">One or more permissions to check for authorization. At least one permission must be specified.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if all
        /// specified permissions are granted; otherwise, <see langword="false"/>.</returns>
        Task<ReturnResponse> AuthorizeAsync(object? resource, OperatorMode operatorMode, params PermissionType[] permissions);
        /// <summary>
        /// Asynchronously determines whether the specified permission is granted for the given resource.
        /// </summary>
        /// <param name="resource">The resource for which access is being checked. Cannot be null.</param>
        /// <param name="permission">The permission to evaluate for the specified resource.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
        /// permission is granted; otherwise, <see langword="false"/>.</returns>
        Task<ReturnResponse> AuthorizeAsync(object? resource, PermissionType permission);
    }
}
