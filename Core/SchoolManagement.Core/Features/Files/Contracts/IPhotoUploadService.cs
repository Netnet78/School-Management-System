using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Application.Interfaces
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
