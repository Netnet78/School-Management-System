using SchoolManagement.Core.Shared.Enums;
using System.Linq.Expressions;

namespace SchoolManagement.Core.Shared.Models
{
    public class FilterCondition<T>
    {
        public Expression<Func<T, object>> Property { get; set; } = default!;
        public FilterOperator Operator { get; set; }
        public object? Value { get; set; }
        public IEnumerable<object>? Values { get; set; }

        public FilterCondition(Expression<Func<T, object>> expression , FilterOperator op, object? value)
        {
            Property = expression;
            Operator = op;
            Value = value;
        }
        public FilterCondition(
            Expression<Func<T, object>> expression,
            FilterOperator op,
            IEnumerable<object>? values)
        {
            Property = expression;
            Operator = op;
            Values = values;
        }
    }
}
