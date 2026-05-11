using SchoolManagement.Core.Models;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Application.Interfaces
{
    public interface IPhotoFetchService
    {
        Task<ReturnResponse<FileObject>> GetStudentPhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default);
        Task<ReturnResponse<FileObject>> GetEmployeePhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default);
    }
}
