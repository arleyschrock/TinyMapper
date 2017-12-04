using System.Linq.Expressions;
using System.Reflection;

namespace TinyMapper
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo ToPropertyInfo(this LambdaExpression expression)
        {
            return (expression.Body as MemberExpression)?.Member as PropertyInfo;
        }
    }
}