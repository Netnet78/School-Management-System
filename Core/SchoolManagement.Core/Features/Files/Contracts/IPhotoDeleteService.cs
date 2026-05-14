using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Features.Files.Contracts
{
    public interface IPhotoDeleteService
    {
        Task<ReturnResponse> DeleteStudentPhoto(Candidate student);
        Task<ReturnResponse> DeleteEmployeePhoto(string photoKey);
    }
}
