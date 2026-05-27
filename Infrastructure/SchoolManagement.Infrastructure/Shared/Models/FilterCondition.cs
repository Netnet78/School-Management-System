using System.Linq.Expressions;

namespace SchoolManagement.Infrastructure.Features.Shared.Models
{
    public class FilterCondition<T>
    {
        public string PropertyPath { get; }
        public FilterOperator Operator { get; }
        public object? Value { get; }
        public IEnumerable<object>? Values { get; }

        public FilterCondition(string propertyPath, FilterOperator op, object? value)
        {
            PropertyPath = propertyPath;
            Operator = op;
            Value = value;
        }

        public FilterCondition(string propertyPath, FilterOperator op, IEnumerable<object>? values)
        {
            PropertyPath = propertyPath;
            Operator = op;
            Values = values;
        }

        public FilterCondition(Expression<Func<T, object?>> expression, FilterOperator op, object? value)
        {
            PropertyPath = PropertyPathHelper.GetPropertyPath(expression);
            Operator = op;
            Value = value;
        }

        public FilterCondition(Expression<Func<T, object?>> expression, FilterOperator op, IEnumerable<object>? values)
        {
            PropertyPath = PropertyPathHelper.GetPropertyPath(expression);
            Operator = op;
            Values = values;
        }
    }
}
