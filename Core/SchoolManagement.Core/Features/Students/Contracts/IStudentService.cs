using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Application.Interfaces
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
