using SchoolManagement.Core.Features.Files.Models;
using SchoolManagement.Core.Features.Files.Enums;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Features.Files.Contracts
{
    public interface IPhotoFetchService
    {
        Task<ReturnResponse<FileObject>> GetStudentPhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default);
        Task<ReturnResponse<FileObject>> GetEmployeePhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default);
    }
}
