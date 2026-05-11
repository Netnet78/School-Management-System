using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Application.Interfaces
{
    public interface IPhotoDeleteService
    {
        Task<ReturnResponse> DeleteStudentPhoto(Candidate student);
        Task<ReturnResponse> DeleteEmployeePhoto(string photoKey);
    }
}
