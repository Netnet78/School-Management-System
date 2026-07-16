using System.Linq.Expressions;

namespace SchoolManagement.Infrastructure.Features.Shared.Models
{
    public class SortCriteria<T>
    {
        public string PropertyPath { get; }
        public OrderDirection Direction { get; }

        public SortCriteria(string propertyPath, OrderDirection direction = OrderDirection.Ascending)
        {
            PropertyPath = propertyPath;
            Direction = direction;
        }

        public SortCriteria(Expression<Func<T, object?>> expression, OrderDirection direction = OrderDirection.Ascending)
        {
            PropertyPath = PropertyPathHelper.GetPropertyPath(expression);
            Direction = direction;
        }
    }
}
