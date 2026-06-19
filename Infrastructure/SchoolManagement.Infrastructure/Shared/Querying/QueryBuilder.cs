using System.Linq.Expressions;
using System.Reflection;

namespace SchoolManagement.Infrastructure.Shared.Querying;

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
    /// <param name="filters">A collection of filter conditions that define the properties, values, 
    /// and operators to use for filtering the
    /// query.</param>
    /// <returns>An <see cref="IQueryable{T}"/> representing the filtered query. 
    /// The returned query includes all filters specified in the filters collection.</returns>
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        params IEnumerable<FilterCondition<T>> filters) where T : IEntity
    {
        foreach (FilterCondition<T> filter in filters)
        {
            if (filter.CustomExpression != null)
            {
                query = query.Where(filter.CustomExpression);
                continue;
            }

            if (filter.Value == null &&
                (filter.Values == null || !filter.Values.Any()) &&
                filter.Operator != FilterOperator.IsNull &&
                filter.Operator != FilterOperator.IsNotNull)
            {
                continue;
            }

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            Expression property = filter.PropertyPath
                .Split('.')
                .Aggregate((Expression)parameter, Expression.PropertyOrField);

            if (property is UnaryExpression unary
                && unary.NodeType == ExpressionType.Convert)
            {
                property = unary.Operand;
            }

            Type propertyType = property.Type;

            Type actualType =
                Nullable.GetUnderlyingType(propertyType)
                ?? propertyType;

            object? convertedValue = null;
            ConstantExpression? constant = null;

            if (filter.Value != null)
            {
                if (actualType.IsEnum)
                {
                    convertedValue = Enum.ToObject(actualType, Convert.ToInt64(filter.Value));
                }
                else
                {
                    convertedValue = Convert.ChangeType(
                        filter.Value,
                        actualType
                    );
                }

                constant = Expression.Constant(convertedValue, propertyType);
            }

            Expression? body = filter.Operator switch
            {
                FilterOperator.Equals => filter.Value == null
                    ? Expression.Equal(property, Expression.Constant(null))
                    : Expression.Equal(property, constant!),

                FilterOperator.NotEquals => filter.Value == null
                    ? Expression.NotEqual(property, Expression.Constant(null))
                    : Expression.NotEqual(property, constant!),

                FilterOperator.GreaterThan =>
                    BuildComparison(
                        property,
                        constant!,
                        ExpressionType.GreaterThan),

                FilterOperator.GreaterThanOrEqual =>
                    BuildComparison(
                        property,
                        constant!,
                        ExpressionType.GreaterThanOrEqual),

                FilterOperator.LessThan =>
                    BuildComparison(
                        property,
                        constant!,
                        ExpressionType.LessThan),

                FilterOperator.LessThanOrEqual =>
                    BuildComparison(
                        property,
                        constant!,
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

        MethodInfo toLowerMethod = typeof(string)
            .GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;

        Expression propertyToLower = Expression.Call(property, toLowerMethod);

        string? loweredValue = value?.ToString()?.ToLower();

        MethodInfo method = typeof(string)
            .GetMethod(methodName, [typeof(string)])!;

        Expression notNull = Expression.NotEqual(
            property,
            Expression.Constant(null, typeof(string))
        );

        Expression constant = Expression.Constant(loweredValue, typeof(string));

        Expression call = Expression.Call(
            propertyToLower,
            method,
            constant
        );

        return Expression.AndAlso(notNull, call);
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        IEnumerable<SortCriteria<T>>? sortCriteria)
    {
        if (sortCriteria == null)
        {
            return query;
        }

        bool first = true;

        foreach (SortCriteria<T> criteria in sortCriteria)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            Expression property = criteria.PropertyPath
                .Split('.')
                .Aggregate((Expression)parameter, Expression.PropertyOrField);

            Expression converted = Expression.Convert(property, typeof(object));

            Type parameterType = typeof(Func<,>).MakeGenericType(typeof(T), typeof(object));
            LambdaExpression lambda = Expression.Lambda(parameterType, converted, parameter);

            string methodName = first
                ? (criteria.Direction == OrderDirection.Ascending ? "OrderBy" : "OrderByDescending")
                : (criteria.Direction == OrderDirection.Ascending ? "ThenBy" : "ThenByDescending");

            query = (IQueryable<T>)typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), typeof(object))
                .Invoke(null, [query, lambda])!;

            first = false;
        }

        return query;
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
            .Select(v => 
            {
                if (v == null)
                    return null;

                if (actualType.IsEnum)
                {
                    return v is string stringValue
                        ? Enum.Parse(actualType, stringValue, ignoreCase: true)
                        : Enum.ToObject(actualType, Convert.ToInt64(v));
                }
                else
                {
                    return Convert.ChangeType(v, actualType);
                }
            })
            .ToList();

        var enumerableType =
            typeof(IEnumerable<>).MakeGenericType(propertyType);

        Array typedArray = Array.CreateInstance(propertyType, convertedValues.Count);
        for (int i = 0; i < convertedValues.Count; i++)
        {
            typedArray.SetValue(convertedValues[i], i);
        }

        ConstantExpression constant = Expression.Constant(
            typedArray,
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
