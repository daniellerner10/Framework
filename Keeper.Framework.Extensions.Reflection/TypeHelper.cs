using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Keeper.Framework.Extensions.Reflection
{
    public static class TypeHelper
    {
        public static MethodCallModel GetMethodCallInfo<TClass, TResult>(Expression<Func<TClass, TResult>> methodCall)
        {
            if (methodCall.Body is not MethodCallExpression methodCallExpression)
                throw new ArgumentException($"methodCall body expression must be of type MethodCallExpression");

            var classType = typeof(TClass);

            return new()
            {
                ClassName = classType.FullName ?? throw new NotSupportedException($"Can not find FullName for type {classType}"),
                AssemblyQualifiedName = classType.AssemblyQualifiedName,
                MethodName = methodCallExpression.Method.Name,
                Arguments = methodCallExpression.Arguments.Select(GetArgumentValue).ToList(),
                ReturnValue = methodCall.ReturnType
            };
        }

        private static object? GetArgumentValue(Expression expression) => expression switch
        {
            ConstantExpression constantExpression => GetArgumentValue(constantExpression),
            MemberExpression memberExpression => GetArgumentValue(memberExpression),
            _ => throw new NotImplementedException($"Can not convert an expression of type {expression.GetType().FullName} to a value.")
        };

        private static object? GetArgumentValue(ConstantExpression expression) => expression.Value;

        private static object? GetArgumentValue(MemberExpression expression) => (expression.Member, expression.Expression) switch
        {
            (FieldInfo fi, ConstantExpression c) => fi.GetValue(c.Value),
            (PropertyInfo pi, ConstantExpression c) => pi.GetValue(c.Value),
            (FieldInfo fi, MemberExpression m) => fi.GetValue(GetArgumentValue(m)),
            (PropertyInfo pi, MemberExpression m) => pi.GetValue(GetArgumentValue(m)),
            _ => throw new NotImplementedException($"Can not get value from a member expression of type {expression.Member.GetType().FullName} with an expression of type {expression.Expression?.GetType().FullName}")
        };
    }
}
