using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class ClassSubjectService : CrudServiceBase<ClassSubject>, IClassSubjectService
    {
        public ClassSubjectService(IClassSubjectRepository repository) : base(repository)
        {
        }
    }
}
