namespace SchoolManagement.Application.Features.Students.Contracts
{
    public interface IStudentQRService : ICrudService<StudentQR>
    {
        Task<ReturnResponse<StudentQR>> GetQRByStudentId(int id);
    }
}
