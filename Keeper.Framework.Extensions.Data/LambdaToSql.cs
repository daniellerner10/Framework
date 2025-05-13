using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Keeper.Framework.Extensions.Data
{
    public class LambdaToSql(
        DbContext _context,
        Dictionary<Type, string> _aliasLookup) : ExpressionVisitor
    {
        private static readonly MemoryCache _propertyNameToColumnNameMapCache = new(new MemoryCacheOptions());

        private const string Null = "NULL";
        private const char SingleQuote = '\'';

        private readonly StringBuilder _builder = new();
        private bool _withAlias = true;
        public Dictionary<int, string> FieldMap = [];

        public string Translate(Expression expression, bool withAlias = true)
        {
            _builder.Clear();
            FieldMap.Clear();

            _withAlias = withAlias;
            Visit(expression);
            _withAlias = true;

            return _builder.ToString();
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _builder.Append(" NOT ");
                    Visit(node.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", node.NodeType));
            }

            return node;
        }

        private int _binaryDepth = 0;
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var localBinaryDepth = _binaryDepth;

            if (localBinaryDepth > 0 && node.NodeType is ExpressionType.And or ExpressionType.AndAlso or ExpressionType.Or or ExpressionType.OrElse)
                _builder.Append('(');

            _binaryDepth++;

            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.And or ExpressionType.AndAlso:
                    _builder.Append(" AND ");
                    break;
                case ExpressionType.Or or ExpressionType.OrElse:
                    _builder.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    if (IsNullConstant(node.Right))
                        _builder.Append(" IS NOT DISTINCT FROM ");
                    else
                        _builder.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    if (IsNullConstant(node.Right))
                        _builder.Append(" IS DISTINCT FROM ");
                    else
                        _builder.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    _builder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _builder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _builder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _builder.Append(" >= ");
                    break;
                case ExpressionType.Add:
                    if (ShouldConcat(node.Right) || ShouldConcat(node.Left))
                        _builder.Append(" || ");
                    else
                        _builder.Append(" + ");

                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", node.NodeType));
            }

            Visit(node.Right);

            if (localBinaryDepth > 0 && node.NodeType is ExpressionType.And or ExpressionType.AndAlso or ExpressionType.Or or ExpressionType.OrElse)
                _builder.Append(')');

            _binaryDepth--;

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var q = node.Value as IQueryable;

            if (q is null && node.Value == null)
                _builder.Append(Null);
            else if (q is null && node.Value is not null)
                WriteObject(node.Value);

            return node;
        }

        private void WriteObject(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.DBNull:
                    _builder.Append(Null);
                    break;
                case TypeCode.Boolean:
                    _builder.Append(((bool)value) ? 1 : 0);
                    break;
                case TypeCode.String:
                case TypeCode.Char:
                    _builder.Append(SingleQuote);
                    _builder.Append((value.ToString() ?? string.Empty).Replace("'", "''"));
                    _builder.Append(SingleQuote);
                    break;

                case TypeCode.DateTime:
                    _builder.Append(SingleQuote);
                    _builder.Append(value);
                    _builder.Append(SingleQuote);
                    break;

                case TypeCode.Object:
                    throw new NotSupportedException($"constant for type '{value?.GetType().FullName}' is not supported");

                default: // it is a number
                    _builder.Append(value);
                    break;
            }
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            base.VisitMemberAssignment(node);

            _builder.Append($" \"{node.Member.Name}\",");

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var members = node.Members!.ToList();

            for (var i = 0; i< node.Arguments.Count; i++)
            {
                if (i > 0)
                    _builder.Append(", ");

                MemberExpression? argument = default;

                switch (node.Arguments[i])
                {
                    case MemberExpression a:
                        argument = a;
                        Visit(argument);
                        break;
                    case MethodCallExpression methodCallExpression:
                        Visit(methodCallExpression);
                        break;
                    default:
                        throw new NotSupportedException($"Argument of type '{node.Arguments[i].GetType().FullName}' is not supported.");
                }

                var member = members[i];

                if (member.Name != argument?.Member.Name)
                {
                    FieldMap.Add(i, member.Name);
                    _builder.Append($" \"{member.Name}\"");
                }
                else if (argument?.Member.Name is not null)
                    FieldMap.Add(i, argument.Member.Name);
                else
                    throw new NotSupportedException("Can not establish name of field.");
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (TryGetSqlMethodName(node.Method, out var sqlMethodName))
            {
                _builder.Append(sqlMethodName);
                _builder.Append('(');

                var arguments = node
                    .Arguments
                    .Skip(1)
                    .SelectMany(x => {
                        if (x is NewArrayExpression newArrayExpression)
                            return newArrayExpression.Expressions;
                        else
                            return Enumerable.Repeat(x, 1);
                    })
                    .ToList();

                for(var i = 0; i < arguments.Count; i++)
                {
                    if (i > 0)
                        _builder.Append(", ");

                    Visit(arguments[i]);
                }

                _builder.Append(')');
            }
            else
                throw new NotSupportedException($"Method '{node.Method}' is not supported");

            return node;
        }

        private static bool TryGetSqlMethodName(MethodInfo methodInfo, [NotNullWhen(true)] out string? sqlMethodName)
        {
            sqlMethodName = methodInfo.GetCustomAttribute<PgMethodAttribute>()?.MethodName;
            return sqlMethodName is not null;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            for (var i = 0; i < node.Bindings.Count; i++)
            {
                var binding = node.Bindings[i];
                if (binding is MemberAssignment assignment)
                {
                    if (i > 0)
                        _builder.Append(", ");

                    MemberExpression? memberExpression = default;

                    switch (assignment.Expression)
                    {
                        case MemberExpression m:
                            memberExpression = m;
                            Visit(memberExpression);

                            break;

                        case MethodCallExpression methodCallExpression:
                            Visit(methodCallExpression);

                            break;
                        default:
                            throw new NotSupportedException($"Bindings of type MemberAssignment must have and expression of type MemberExpression.  Type '{assignment.Expression.GetType().FullName}' is not supported.");
                    }

                    if (assignment.Member.Name != memberExpression?.Member.Name)
                    {
                        FieldMap.Add(i, assignment.Member.Name);
                        _builder.Append($" \"{assignment.Member.Name}\"");
                    }
                    else if (memberExpression?.Member.Name is not null)
                        FieldMap.Add(i, memberExpression.Member.Name);
                    else
                        throw new NotSupportedException("Can not establish name of field.");
                }
                else
                    throw new NotSupportedException($"Bindings of type '{binding.GetType().FullName}' is not supported.");
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression is not null && m.Member.DeclaringType is not null)
            {
                switch (m.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        if (GetPropertyNameToColumnNameMap(_context, m.Member.DeclaringType).TryGetValue(m.Member.Name, out var columnName))
                        {
                            if (_withAlias)
                            {
                                if (!_aliasLookup.TryGetValue(m.Member.DeclaringType, out var alias))
                                    alias = _context.GetTableName(m.Member.DeclaringType);

                                _builder.Append($"{alias}.\"{columnName}\"");
                            }
                            else
                                _builder.Append($"\"{columnName}\"");

                            return m;
                        }
                        else
                            throw new NotSupportedException($"The member '{m.Member.Name}' is not found in entity '{m.Member.DeclaringType.FullName}'");
                    case ExpressionType.Constant:
                        var exp = (ConstantExpression)m.Expression;
                        var value = m.Member switch
                        {
                            FieldInfo fieldInfo => fieldInfo.GetValue(exp.Value) ?? throw new NotSupportedException($"Can not get value of field '{m.Member}'"),
                            PropertyInfo propertyInfo => propertyInfo.GetValue(exp.Value) ?? throw new NotSupportedException($"Can not get value of property '{m.Member}'"),
                            _ => throw new NotSupportedException($"The member '{m.Member.MemberType}' is not supported"),
                        };

                        WriteObject(value);

                        return m;
                    default:
                        throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
                }
            }
            else if (m.Member is PropertyInfo propertyInfo)
            {
                switch (propertyInfo.Name)
                {
                    case nameof(DateTime.Now):
                        _builder.Append("now()");
                        break;
                    case nameof(DateTime.UtcNow):
                        _builder.Append("now() at time zone 'utc'");
                        break;
                    default:
                        base.VisitMember(m);
                        break;
                }
                return m;
            }
            else if (m.Member.Name == "Functions")
                return base.VisitMember(m);
            else
                throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }

        protected static bool IsNullConstant(Expression exp) =>
            exp is ConstantExpression constExp && (constExp.Value == null || constExp.Value == DBNull.Value);

        protected static bool ShouldConcat(Expression exp) => exp switch
        {
            MemberExpression memberExpression => 
                TryGetPostgresType(memberExpression.Member, out var postgresType) && ShouldConcat(postgresType),
            MethodCallExpression methodCallExpression => 
                TryGetPostgresType(methodCallExpression.Method, out var postgresType) && ShouldConcat(postgresType),
            _ => false
        };

        protected static bool ShouldConcat(string postgresType) => postgresType switch
        {
            "text" or "jsonb" or "json" => true,
            _ => false
        };

        private static bool TryGetPostgresType(MemberInfo member, [NotNullWhen(true)] out string? postgresType)
        {
            postgresType = member.GetCustomAttribute<ColumnAttribute>()?.TypeName
                        ?? member.GetCustomAttribute<PgMethodAttribute>()?.ReturnType
                        ?? DbContextExtensions.GetPostgresType(member, throwExceptionOnFailure: false);

            return postgresType is not null;
        }

        internal static Dictionary<string, string> GetPropertyNameToColumnNameMap(DbContext context, Type entityType) =>
               _propertyNameToColumnNameMapCache.GetOrCreate(entityType, _ =>
               {
                   if (context.TryGetModel(entityType, out var model))
                       return model
                           .GetProperties()
                           .ToDictionary(
                               x => x.PropertyInfo!.Name,
                               x => x.GetColumnName()
                           );
                   else
                       return entityType
                           .GetProperties()
                           .ToDictionary(
                               x => x.Name,
                               x =>
                               {
                                   var attr = x.GetCustomAttribute<ColumnAttribute>();
                                   if (attr is not null && !string.IsNullOrWhiteSpace(attr.Name))
                                       return attr.Name;
                                   else
                                       return x.Name;
                               }
                           );

               }) ?? throw new InvalidOperationException("Can not get fields of model");
    }
}
