namespace SchoolManagement.Application.Features.Classes.Contracts
{
    public interface IClassSubjectService : ICrudService<ClassSubject>
    {
        Task<ReturnResponse> SyncSubjectsForClassAsync(int classId, IEnumerable<(int SubjectId, int? TeacherId)> subjects);
    }
}
