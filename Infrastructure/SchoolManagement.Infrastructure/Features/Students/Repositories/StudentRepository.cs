using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Querying;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Students.Repositories;

public class StudentRepository : BaseRepository<Student>, IStudentRepository
{
    public StudentRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Student>> FindAsync(
        IEnumerable<FilterCondition<Student>>? filters,
        Expression<Func<Student, bool>>? extraPredicate,
        int? page,
        int? pageSize,
        IEnumerable<SortCriteria<Student>>? orderBy,
        params string[]? includes)
    {
        IQueryable<Student> query = BuildQuery(filters, includes);

        if (extraPredicate != null)
        {
            query = query.Where(extraPredicate);
        }

        query = query.ApplySorting(orderBy);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> CountAsync(
        IEnumerable<FilterCondition<Student>>? filters,
        Expression<Func<Student, bool>>? extraPredicate,
        int? page,
        int? pageSize)
    {
        IQueryable<Student> query = BuildQuery(filters, null);

        if (extraPredicate != null)
        {
            query = query.Where(extraPredicate);
        }

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.CountAsync();
    }
}
