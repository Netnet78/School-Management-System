using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Core.Enums
{
    /// <summary>
    /// Specifies the type of operation to perform.
    /// </summary>
    /// <remarks>Use this enumeration to indicate whether an upload or delete operation should be executed.
    /// This is typically used in APIs that support multiple operation types for resources.</remarks>
    public enum OperationType
    {
        Upload,
        Delete,
    }
}
