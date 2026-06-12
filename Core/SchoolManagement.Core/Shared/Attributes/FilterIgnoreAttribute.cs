namespace SchoolManagement.Core.Shared.Attributes
{
    /// <summary>
    /// An attribute for defining a filter item that should be ignored but still
    /// notifies about the value changes/update
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FilterIgnoreAttribute : Attribute
    {
    }
}
