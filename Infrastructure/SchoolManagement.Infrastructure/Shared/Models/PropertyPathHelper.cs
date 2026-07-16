using System.Linq.Expressions;

namespace SchoolManagement.Infrastructure.Features.Shared.Models
{
    internal static class PropertyPathHelper
    {
        public static string GetPropertyPath<T>(Expression<Func<T, object?>> expression)
        {
            Expression body = expression.Body;

            while (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                body = unary.Operand;
            }

            var parts = new List<string>();
            while (body is MemberExpression member)
            {
                parts.Add(member.Member.Name);
                body = member.Expression!;
            }

            if (parts.Count == 0)
                throw new ArgumentException("Expression must be a property access expression.");

            parts.Reverse();
            return string.Join(".", parts);
        }
    }
}
