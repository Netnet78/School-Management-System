namespace SchoolManagement.Application.Features.Subjects.Contracts
{
    public interface ISubjectService : ICrudService<Subject>
    {
        Task<IEnumerable<SubjectMapper>> GetMappersForSubjectAsync(int subjectId);
    }
}
