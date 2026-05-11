using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class GradeService : CrudServiceBase<Grade>, IGradeService
    {
        public GradeService(IGradeRepository repository) : base(repository)
        {
        }
    }
}
