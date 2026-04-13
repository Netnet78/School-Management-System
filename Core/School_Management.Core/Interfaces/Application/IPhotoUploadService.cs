using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IPhotoUploadService
    {
        Task<FileObject> UploadStudentPhoto(string path);
        Task<FileObject> UploadEmployeePhoto(string path);
    }
}
