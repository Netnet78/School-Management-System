using System.Linq.Expressions;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Students.Contracts
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        public IQueryable Query();

        Task<IEnumerable<Student>> FindAsync(
            IEnumerable<FilterCondition<Student>>? filters,
            Expression<Func<Student, bool>>? extraPredicate,
            int? page,
            int? pageSize,
            IEnumerable<SortCriteria<Student>>? orderBy,
            params string[]? includes);

        Task<int> CountAsync(
            IEnumerable<FilterCondition<Student>>? filters,
            Expression<Func<Student, bool>>? extraPredicate,
            int? page,
            int? pageSize);
    }
}
