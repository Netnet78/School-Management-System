using School_Management.Core.DTOs;
using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IClassService : ICrudService<Class>
    {
        Task<ReturnResponse<IEnumerable<ClassStudentCountDto>>> GetStudentCountPerClassAsync();
    }
}
