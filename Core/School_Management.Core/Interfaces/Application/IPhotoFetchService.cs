using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IPhotoFetchService
    {
        Task<FileObject?> GetStudentPhoto(string photoKey);
        Task<FileObject?> GetEmployeePhoto(string photoKey);
    }
}
