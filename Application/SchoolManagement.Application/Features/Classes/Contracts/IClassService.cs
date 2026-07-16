using SchoolManagement.Application.Features.Classes.Models;

namespace SchoolManagement.Application.Features.Classes.Contracts
{
    public interface IClassService : ICrudService<Class>
    {
        Task<ReturnResponse<IEnumerable<ClassStudentCountDto>>> GetStudentCountPerClassAsync(int fromYear, int toYear);
        Task<ReturnResponse<Class?>> GetByIdWithSubjectsAsync(int id);
        Task<ClassPermissions> GetPermissionsAsync();
    }
}
