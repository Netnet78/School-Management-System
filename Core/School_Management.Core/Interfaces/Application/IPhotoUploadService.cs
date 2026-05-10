using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IPhotoUploadService
    {
        Task<FileObject> UploadStudentPhoto(string path, Candidate student);
        public async Task<FileObject> UploadStudentPhoto(string path, Student student)
        {
            return await UploadStudentPhoto(path, student.Candidate);
        }
        Task<FileObject> UploadEmployeePhoto(string path);
    }
}
