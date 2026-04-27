using School_Management.Core.Models;
using School_Management.Core.Enums;

namespace School_Management.Core.Interfaces.Application
{
    public interface IPhotoFetchService
    {
        Task<FileObject?> GetStudentPhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default);
        Task<FileObject?> GetEmployeePhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default);
    }
}
