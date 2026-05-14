
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Students.Contracts
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        public IQueryable Query();
    }
}

