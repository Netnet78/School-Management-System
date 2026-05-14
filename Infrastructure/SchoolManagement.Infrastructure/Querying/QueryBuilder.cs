using System.Linq.Expressions;
using System.Reflection;

namespace SchoolManagement.Infrastructure.Querying;

public static class QueryBuilder
{
    /// <summary>
    /// Applies a collection of filter conditions to the specified queryable source.
    /// </summary>
    /// <remarks>Filters are applied in the order provided. This method supports a variety of filter
    /// operators, including equality, comparison, string operations, and null checks. The returned query is not
    /// executed until enumerated. This method is intended for use with LINQ providers that support expression trees,
    /// such as Entity Framework.</remarks>
    /// <typeparam name="T">The type of the elements in the queryable source.</typeparam>
    /// <param name="query">The source query to which the filters will be applied.</param>
    /// <param name="filters">A collection of filter conditions that define the properties, values, and operators to use for filtering the
    /// query.</param>
    /// <returns>An IQueryable<T> representing the filtered query. The returned query includes all filters specified in the
    /// filters collection.</returns>
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        IEnumerable<FilterCondition<T>> filters) where T : IEntity
    {
        foreach (FilterCondition<T> filter in filters)
        {
            ParameterExpression parameter = filter.Property.Parameters[0];
            
            Type propertyType = filter.Property.Body.Type;

            Type actualType =
                Nullable.GetUnderlyingType(propertyType)
                ?? propertyType;

            object? convertedValue = null;

            if (filter.Value != null)
            {
                convertedValue = Convert.ChangeType(
                    filter.Value,
                    actualType
                );
            }

            ConstantExpression constant = Expression.Constant(
                convertedValue,
                propertyType
            );

            Expression property = filter.Property.Body;

            Expression? body = filter.Operator switch
            {
                FilterOperator.Equals =>
                    Expression.Equal(property, constant),

                FilterOperator.NotEquals =>
                    Expression.NotEqual(property, constant),

                FilterOperator.GreaterThan =>
                    BuildComparison(
                        property,
                        constant,
                        ExpressionType.GreaterThan),

                FilterOperator.GreaterThanOrEqual =>
                    BuildComparison(
                        property,
                        constant,
                        ExpressionType.GreaterThanOrEqual),

                FilterOperator.LessThan =>
                    BuildComparison(
                        property,
                        constant,
                        ExpressionType.LessThan),

                FilterOperator.LessThanOrEqual =>
                    BuildComparison(
                        property,
                        constant,
                        ExpressionType.LessThanOrEqual),

                FilterOperator.Contains =>
                    BuildStringMethod(
                        property,
                        convertedValue,
                        nameof(string.Contains)),

                FilterOperator.StartsWith =>
                    BuildStringMethod(
                        property,
                        convertedValue,
                        nameof(string.StartsWith)),

                FilterOperator.EndsWith =>
                    BuildStringMethod(
                        property,
                        convertedValue,
                        nameof(string.EndsWith)),

                FilterOperator.In =>
                    BuildInExpression(
                        property,
                        filter.Values),

                FilterOperator.IsNull =>
                    Expression.Equal(
                        property,
                        Expression.Constant(null, propertyType)
                    ),

                FilterOperator.IsNotNull =>
                    Expression.NotEqual(
                        property,
                        Expression.Constant(null, propertyType)
                    ),

                _ => null
            };

            if (body != null)
            {
                Expression<Func<T, bool>> lambda =
                    Expression.Lambda<Func<T, bool>>(body, parameter);

                query = query.Where(lambda);
            }
        }

        return query;
    }

    private static BinaryExpression BuildComparison(
        Expression property,
        ConstantExpression constant,
        ExpressionType comparisonType)
    {
        return Expression.MakeBinary(
            comparisonType,
            property,
            constant
        );
    }

    private static BinaryExpression BuildStringMethod(
        Expression property,
        object? value,
        string methodName)
    {
        if (property.Type != typeof(string))
        {
            throw new InvalidOperationException(
                $"{methodName} can only be used on string properties.");
        }

        MethodInfo method = typeof(string)
            .GetMethod(methodName, [typeof(string)])!;

        Expression notNull = Expression.NotEqual(
            property,
            Expression.Constant(null, typeof(string))
        );

        Expression call = Expression.Call(
            property,
            method,
            Expression.Constant(value, typeof(string))
        );

        return Expression.AndAlso(notNull, call);
    }

    private static MethodCallExpression BuildInExpression(
        Expression property,
        IEnumerable<object>? values)
    {
        if (values == null)
        {
            throw new InvalidOperationException(
                "IN operator requires values.");
        }

        Type propertyType = property.Type;

        Type actualType =
            Nullable.GetUnderlyingType(propertyType)
            ?? propertyType;

        List<object?> convertedValues = values
            .Select(v => v == null
                ? null
                : Convert.ChangeType(v, actualType))
            .ToList();

        var enumerableType =
            typeof(IEnumerable<>).MakeGenericType(propertyType);

        ConstantExpression constant = Expression.Constant(
            convertedValues,
            enumerableType
        );

        MethodInfo containsMethod = typeof(Enumerable)
            .GetMethods()
            .First(m =>
                m.Name == nameof(Enumerable.Contains)
                && m.GetParameters().Length == 2)
            .MakeGenericMethod(propertyType);

        return Expression.Call(
            containsMethod,
            constant,
            property
        );
    }
}