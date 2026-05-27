namespace SchoolManagement.Application.Features.Files.Contracts
{
    public interface IPhotoUploadService
    {
        Task<ReturnResponse<FileObject>> UploadStudentPhoto(string path, Candidate student);
        public async Task<ReturnResponse<FileObject>> UploadStudentPhoto(string path, Student student)
        {
            return await UploadStudentPhoto(path, student.Candidate);
        }
        Task<ReturnResponse<FileObject>> UploadEmployeePhoto(string path, Employee employee);
    }
}
