namespace SchoolManagement.Application.Features.Subjects.Services
{
    public class SubjectService : CrudServiceBase<Subject>, ISubjectService
    {
        public SubjectService(ISubjectRepository repository) : base(repository)
        {
        }
    }
}


