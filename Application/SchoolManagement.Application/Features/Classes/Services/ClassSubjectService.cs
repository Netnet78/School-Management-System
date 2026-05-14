namespace SchoolManagement.Application.Features.Classes.Services
{
    public class ClassSubjectService : CrudServiceBase<ClassSubject>, IClassSubjectService
    {
        public ClassSubjectService(IClassSubjectRepository repository) : base(repository)
        {
        }
    }
}


