using System.Linq.Expressions;

namespace SchoolManagement.Infrastructure.Shared.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }
    }
}
