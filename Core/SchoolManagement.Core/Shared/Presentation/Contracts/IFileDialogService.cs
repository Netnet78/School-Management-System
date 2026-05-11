using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface IFileDialogService
    {
        /// <summary>
        /// Shows the user the dialog option to choose any files in Windows
        /// </summary>
        /// <param name="title"></param>
        /// <param name="filterLabel"></param>
        /// <param name="filters"></param>
        /// <param name="multiSelect"></param>
        /// <returns>A <see cref="FileDialogObject"/> that can be used to describe the selected files in the dialog after pressing OK</returns>
        public FileDialogObject ShowDialog(string title, bool multiSelect = false, string? filterLabel = null, params string[]? filters);
        
    }
}
