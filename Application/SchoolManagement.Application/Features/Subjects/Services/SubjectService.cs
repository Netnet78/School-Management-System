using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class SubjectService : CrudServiceBase<Subject>, ISubjectService
    {
        public SubjectService(ISubjectRepository repository) : base(repository)
        {
        }
    }
}
