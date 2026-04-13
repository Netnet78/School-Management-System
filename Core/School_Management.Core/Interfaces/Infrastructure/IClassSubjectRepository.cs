using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IClassSubjectRepository
    {
        Task<List<ClassSubject>> GetAllAsync();
        Task<ClassSubject?> GetByIdAsync(int id);
        Task AddAsync(ClassSubject classSubject);
        Task UpdateAsync(ClassSubject classSubject);
        Task DeleteAsync(ClassSubject classSubject);
        Task SaveAsync();
    }
}