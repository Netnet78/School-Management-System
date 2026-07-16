using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Subjects.Contracts
{
    public interface ISubjectRepository : IBaseRepository<Subject>
    {
        Task<Subject?> GetByIdWithMappersAsync(int id);
        Task<SubjectComponent?> FindComponentByNameAsync(string name);
        Task<Dictionary<string, SubjectComponent>> FindComponentsByNamesAsync(IEnumerable<string> names);
        Task<IEnumerable<SubjectMapper>> GetMappersForSubjectAsync(int subjectId);
    }
}
