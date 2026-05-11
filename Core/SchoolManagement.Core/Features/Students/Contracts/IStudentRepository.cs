using SchoolManagement.Core.Models;
using System.Linq.Expressions;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        public IQueryable Query();
    }
}
