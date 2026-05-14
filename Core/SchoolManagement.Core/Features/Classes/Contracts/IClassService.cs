using SchoolManagement.Core.Features.Classes.DTOs;
using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Features.Classes.Contracts
{
    public interface IClassService : ICrudService<Class>
    {
        Task<ReturnResponse<IEnumerable<ClassStudentCountDto>>> GetStudentCountPerClassAsync();
    }
}

