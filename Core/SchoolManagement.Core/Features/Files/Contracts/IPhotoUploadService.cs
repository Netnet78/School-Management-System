

using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Core.Features.Files.Models;
using SchoolManagement.Core.Features.Students.Models;

namespace SchoolManagement.Core.Features.Files.Contracts
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

