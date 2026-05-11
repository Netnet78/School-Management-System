using SchoolManagement.Core.Enums;
using System.Linq.Expressions;

namespace SchoolManagement.Core.Shared.Models
{
    public class FilterCondition<T>
    {
        public Expression<Func<T, object>> Property { get; set; } = default!;
        public FilterOperator Operator { get; set; }
        public object? Value { get; set; }
        public IEnumerable<object>? Values { get; set; }
    }
}
