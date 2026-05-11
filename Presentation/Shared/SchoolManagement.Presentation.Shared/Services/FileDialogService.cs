using Microsoft.Win32;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Presentation.Contracts;

namespace SchoolManagement.Presentation.Shared.Services
{
    public class FileDialogService : IFileDialogService
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public FileDialogObject ShowDialog(string title, bool multiSelect = false, string ? filterLabel = null, params string[]? filters)
        {
            filters = filters?.Select(f => "*." + f).ToArray();
            string filterNames = string.Join(";", filters ?? []);
            string label = $"{filterLabel} ({filterNames})|{filterNames}";

            OpenFileDialog openFileDialog = new()
            {
                Filter = label,
                Title = title,
                Multiselect = multiSelect
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileObject[] files = openFileDialog.FileNames.Select(f => new FileObject(f)).ToArray();
                return new(files);
            }
            else
            {
                FileObject? file = null;
                return new(file);
            }
        }
    }
}
