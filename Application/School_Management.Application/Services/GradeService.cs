using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class GradeService : CrudServiceBase<Grade>, IGradeService
    {
        public GradeService(IGradeRepository repository) : base(repository)
        {
        }
    }
}
