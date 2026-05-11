using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Shared.Models;
using System.Linq.Expressions;

namespace SchoolManagement.Core.Shared.Querying;

public static class QueryBuilder
{
    public static IQueryable<T> ApplyFilters<T>(
        IQueryable<T> query,
        IEnumerable<FilterCondition<T>> filters)
    {
        foreach (var filter in filters)
        {
            var parameter = filter.Property.Parameters[0];
            var member = Expression.Convert(filter.Property.Body, typeof(object));

            Expression? body = filter.Operator switch
            {
                FilterOperator.Equals =>
                    Expression.Equal(
                        Expression.Convert(filter.Property.Body, filter.Value!.GetType()),
                        Expression.Constant(filter.Value)
                    ),

                FilterOperator.Contains =>
                    Expression.Call(
                        filter.Property.Body,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) })!,
                        Expression.Constant(filter.Value)
                    ),

                FilterOperator.GreaterThan =>
                    Expression.GreaterThan(
                        Expression.Convert(filter.Property.Body, typeof(IComparable)),
                        Expression.Constant(filter.Value)
                    ),

                FilterOperator.LessThan =>
                    Expression.LessThan(
                        Expression.Convert(filter.Property.Body, typeof(IComparable)),
                        Expression.Constant(filter.Value)
                    ),

                FilterOperator.In =>
                    Expression.Call(
                        typeof(Enumerable).GetMethods()
                            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(filter.Property.Body.Type),
                        Expression.Constant(filter.Values),
                        filter.Property.Body
                    ),

                _ => null
            };

            if (body != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
                query = query.Where(lambda);
            }
        }

        return query;
    }
}