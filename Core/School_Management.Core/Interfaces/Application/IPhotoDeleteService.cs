using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IPhotoDeleteService
    {
        Task<ReturnResponse> DeleteStudentPhoto(Candidate student);
        Task<ReturnResponse> DeleteEmployeePhoto(string photoKey);
    }
}
