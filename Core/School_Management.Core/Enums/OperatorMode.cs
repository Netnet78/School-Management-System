namespace School_Management.Core.Enums
{
    /// <summary>
    /// Specifies the logical operator mode used to evaluate multiple conditions.
    /// </summary>
    /// <remarks>Use this enumeration to indicate whether all conditions must be met (AND) or if any condition
    /// being met is sufficient (OR) when performing logical evaluations.</remarks>
    public enum OperatorMode
    {
        /// <summary>
        /// Returns true if all of the conditions are true
        /// </summary>
        AND,
        /// <summary>
        /// Returns true if one or more conditions are true
        /// </summary>
        OR
    }
}
