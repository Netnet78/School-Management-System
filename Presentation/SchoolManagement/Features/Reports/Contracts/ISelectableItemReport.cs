namespace SchoolManagement.Presentation.Features.Reports.Contracts;

/// <summary>
/// Represents a report that supports selecting and deselecting items, as well as toggling the selection state for all
/// items.
/// </summary>
/// <remarks>Implementations of this interface allow consumers to manage selection state for a collection of
/// items, typically in user interface scenarios such as lists or grids. The interface provides methods to select or
/// deselect individual items by their identifier and to toggle the selection state of all items at once.</remarks>
public interface ISelectableItemReport
{
    bool IsAllSelected { get; }
    bool ShowSelectedOnly { get; }
    void SelectItem(int id);
    void DeselectItem(int id);
    void ToggleSelectAll();
}
