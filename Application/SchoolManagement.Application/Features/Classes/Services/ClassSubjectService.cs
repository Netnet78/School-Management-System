using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class ClassSubjectService : CrudServiceBase<ClassSubject>, IClassSubjectService
    {
        public ClassSubjectService(IClassSubjectRepository repository) : base(repository)
        {
        }
    }
}
