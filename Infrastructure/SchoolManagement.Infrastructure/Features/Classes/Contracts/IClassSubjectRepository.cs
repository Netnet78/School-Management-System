using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Classes.Contracts
{
    public interface IClassSubjectRepository : IBaseRepository<ClassSubject>
    {
        Task SyncForClassAsync(int classId, IEnumerable<(int SubjectId, int? TeacherId)> subjects);
    }
}
