using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IStudentService : ICrudService<Student>
    {
        public Task<ReturnResponse<IEnumerable<Student>>> GetStudentsAsync(int page, int pageSize, StudentFilterOptions filterOptions);
        public Task<ReturnResponse<int>> GetStudentsCount(int page, int pageSize, StudentFilterOptions filterOptions);
        public Task<ReturnResponse> InsertStudentAsync(Student student);
        public Task<ReturnResponse> UpdateStudentAsync(Student student);
        public Task<ReturnResponse> DeleteStudentAsync(Student student);
    }
}
