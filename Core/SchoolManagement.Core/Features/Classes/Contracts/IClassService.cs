using SchoolManagement.Core.Application.DTOs;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Application.Interfaces
{
    public interface IClassService : ICrudService<Class>
    {
        Task<ReturnResponse<IEnumerable<ClassStudentCountDto>>> GetStudentCountPerClassAsync();
    }
}
