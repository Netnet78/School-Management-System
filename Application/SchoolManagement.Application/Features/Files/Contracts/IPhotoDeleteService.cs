namespace SchoolManagement.Application.Features.Files.Contracts
{
    public interface IPhotoDeleteService
    {
        Task<ReturnResponse> DeleteStudentPhoto(Candidate student);
        Task<ReturnResponse> DeleteEmployeePhoto(Employee employee);
    }
}
