using School_Management.Core.Models;
using System.Linq.Expressions;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        public IQueryable Query();
    }
}
