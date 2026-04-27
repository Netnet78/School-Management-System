using System.Linq.Expressions;

namespace School_Management.Core.Helpers
{
    public static class QueryHelper
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
