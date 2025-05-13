using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Keeper.Framework.Collections;
using Keeper.Framework.Extensions.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using static Keeper.Framework.Extensions.Data.DbContextExtensions;

namespace Keeper.Framework.Extensions.Data
{
    public static class DbContextExtensions
    {
        internal static Func<DbContext, DbConnection> GetConnection = c => c.Database.GetDbConnection();

        internal static Func<string, DbConnection, CancellationToken, Task<IBinaryWriter>> BeginBinaryImportAsync =
            async (string sql, DbConnection connection, CancellationToken cancellationToken) =>
            {
                if (connection is NpgsqlConnection npgsqlConnection)
                    return new NpgSqlBinaryWriter(await npgsqlConnection.BeginBinaryImportAsync(sql, cancellationToken));
                else
                    throw new NotSupportedException("BeginBinaryImportAsync can only be called on a NpgsqlConnection");
            };

        internal static Func<DbContext, CancellationToken, Task<IDbContextTransaction>> BeginTransaction = 
            (DbContext context, CancellationToken cancellationToken) => context.Database.BeginTransactionAsync(cancellationToken);

        internal static Func<DbContext, IDbContextTransaction?> GetCurrentTransaction =
            (DbContext context) => context.Database.CurrentTransaction;

        private readonly static Random rnd = new();
        internal static Func<int, string> GetRandomTableSuffix =
            (int length) =>
            {
                const string chars = "abcdefghijklmnopqrstuvwxyz";

                return new string(
                    Enumerable
                        .Repeat(chars, length)
                        .Select(s => s[rnd.Next(s.Length)])
                        .ToArray()
                );
            };

        private static readonly MemoryCache _columnNameCache = new(new MemoryCacheOptions());

        public static Task<ulong> BulkInsertAsync<TEntity>(this DbContext context, IEnumerable<TEntity> list, CancellationToken cancellationToken = default)
            where TEntity : class =>
                context.BulkInsertAsync(list.ToAsyncEnumerable(), cancellationToken);

        public static async Task<ulong> BulkInsertAsync<TEntity>(this DbContext context, IAsyncEnumerable<TEntity> list, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var tableName = context.GetTableName<TEntity>();

            return await context
                .BulkInsertAsync(
                    tableName,
                    list,
                    cancellationToken
                );
        }

        private static async Task<ulong> BulkInsertAsync<TEntity>(
            this DbContext context, 
            string tableName,
            IAsyncEnumerable<TEntity> list, 
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var columns = context.GetColumnNamesDictionary<TEntity>(onlyEscaped: true);

            var sql = $"COPY {tableName} ({$"{string.Join(",", columns.Keys)}"}) FROM STDIN BINARY;";

            var connection = GetConnection(context) ?? throw new InvalidOperationException("Can not get DbConnection");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            await using var writer = await BeginBinaryImportAsync(sql, connection, cancellationToken);
            var values = new object[columns.Count];

            await foreach (var item in list.WithCancellation(cancellationToken))
            {
                for (var i = 0; i < columns.Count; i++)
                    values[i] = columns[i].Value.Getter(item);

                await writer.WriteRowAsync(cancellationToken, values);
            }

            var rows = await writer.CompleteAsync(cancellationToken);

            return rows;
        }

        public static async IAsyncEnumerable<TEntity> CastToEntity<TContext, TSelect, TEntity>(
            this TContext context,
            IAsyncEnumerable<TSelect> list, 
            Func<TSelect, Task<TEntity>> map,
            Func<TSelect, Exception?, Task>? onError,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
                where TContext : DbContext
                where TEntity : class
        {
            var entityType = typeof(TEntity);

            var designTimeModel = context.GetInfrastructure().GetRequiredService<IDesignTimeModel>();
            var model = designTimeModel.Model.FindEntityType(entityType)
                ?? throw new NotSupportedException($"Can not find entity '{entityType.FullName}'");

            var index = 0;
            await foreach (var item in list.WithCancellation(cancellationToken))
            {
                index++;

                var (entity, exception) = await GetEntity(item, map);
                if (entity is null)
                {
                    if (onError is not null)
                        await onError(item, exception);
                }
                else
                {
                    var validationErrors = GetEntityValidationErrors(entity, model);
                    if (validationErrors.Count == 0)
                        yield return entity;
                    else if (onError is not null)
                    {
                        exception = new ValidationException($"Item {index} of list has a validation error(s):\n{string.Join("\n", validationErrors)}");
                        await onError(item, exception);
                    }
                }
            }
        }

        internal static List<string> GetEntityValidationErrors<TEntity>(TEntity entity, IEntityType model) where TEntity : class
        {
            var validationErrors = new List<string>();
            var properties = model.GetFlattenedProperties();

            foreach (var property in properties)
            {
                var value = property.GetGetter().GetClrValue(entity);
                if (!property.IsNullable && value is null)
                    validationErrors.Add($"Property '{property.Name}' of type '{model.Name}' is not nullable.");
                else
                {
                    var maxLength = property.GetMaxLength();
                    if (maxLength.HasValue)
                    {
                        var strValue = value?.ToString() ?? string.Empty;
                        if (strValue.Length > maxLength.Value)
                             validationErrors.Add($"Property '{property.Name}' of type '{model.Name}' has max length of '{maxLength.Value}'. Value '{strValue}' has a length of '{strValue.Length}'.");
                    }
                }
            }

            return validationErrors;
        }

        private static async Task<(TEntity? entity, Exception? exception)> GetEntity<TSelect, TEntity>(
            TSelect item, 
            Func<TSelect, Task<TEntity>> map)
                where TEntity : class
        {
            try
            {
                var entity = await map(item);
                return (entity, null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        public static async Task Merge<TContext, TUpdateEntity, TSelectEntity>(
            this TContext context,
            Expression<Func<TContext, DbSet<TUpdateEntity>>> toUpdate,
            IAsyncEnumerable<TSelectEntity> list,
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> predicate,
            Action<SqlActions<TUpdateEntity, TSelectEntity>> setActions,
            CancellationToken cancellationToken = default)
                where TContext : DbContext
                where TUpdateEntity : class
                where TSelectEntity : class
        {
            var (transaction, isNewTransaction) = await context.GetTransaction(cancellationToken);

            try
            {
                var tempTable = await context.PopulateTempTable
                (
                    list, 
                    cancellationToken
                );

                await context.Merge(
                    toUpdate,
                    tempTable,
                    predicate,
                    setActions,
                    cancellationToken
                );

                if (isNewTransaction)
                    await transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                if (isNewTransaction)
                    await transaction.DisposeAsync();
            }
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It is used for selecting update type.")]
        public static async Task Merge<TContext, TUpdateEntity, TSelectEntity>(
            this TContext context,
            Expression<Func<TContext, DbSet<TUpdateEntity>>> toUpdate,
            IQueryable<TSelectEntity> list,
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> predicate,
            Action<SqlActions<TUpdateEntity, TSelectEntity>> setActions,
            CancellationToken cancellationToken = default)
                where TContext : DbContext
                where TUpdateEntity : class
            where TSelectEntity : class
        {
            var connection = GetConnection(context) ?? throw new InvalidOperationException("Can not get DbConnection");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = context.GetMergeSql(
                list,
                predicate,
                setActions,
                out _
            );

            await command.ExecuteNonQueryAsync(cancellationToken);
        }        

        public static async IAsyncEnumerable<TReturnEntity> Merge<TContext, TUpdateEntity, TSelectEntity, TReturnEntity>(
            this TContext context,
            Expression<Func<TContext, DbSet<TUpdateEntity>>> toUpdate,
            IAsyncEnumerable<TSelectEntity> list,
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> predicate,
            Action<SqlActions<TUpdateEntity, TSelectEntity>> setActions,
            Expression<Func<TUpdateEntity, TSelectEntity, TReturnEntity>> returning,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
                where TContext : DbContext
                where TUpdateEntity : class
                where TSelectEntity : class
                where TReturnEntity : class, new()
        {
            var (transaction, isNewTransaction) = await context.GetTransaction(cancellationToken);

            try
            {
                var tempTable = await context.PopulateTempTable
                (
                    list,
                    cancellationToken
                );

                var asyncEnum = context.Merge(
                    toUpdate,
                    tempTable,
                    predicate,
                    setActions,
                    returning,
                    cancellationToken
                );

                await foreach(var item in asyncEnum.WithCancellation(cancellationToken))
                    yield return item;

                if (isNewTransaction)
                    await transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                if (isNewTransaction)
                    await transaction.DisposeAsync();
            }
        }

        public static async IAsyncEnumerable<TReturnEntity> Merge<TContext, TUpdateEntity, TSelectEntity, TReturnEntity>(
            this TContext context,
            Expression<Func<TContext, DbSet<TUpdateEntity>>> toUpdate,
            IQueryable<TSelectEntity> list,
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> predicate,
            Action<SqlActions<TUpdateEntity, TSelectEntity>> setActions,
            Expression<Func<TUpdateEntity, TSelectEntity, TReturnEntity>> returning,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
                where TContext : DbContext
                where TUpdateEntity : class
                where TSelectEntity : class
                where TReturnEntity : class, new()
        {
            var connection = GetConnection(context) ?? throw new InvalidOperationException("Can not get DbConnection");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = context.GetMergeSql(
                toUpdate,
                list,
                predicate,
                setActions,
                returning,
                out var selectFields
            );

            var columnsDictionary = context.GetColumnNamesDictionary<TReturnEntity>(onlyEscaped: false);
            var values = new object[selectFields.Count];

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                reader.GetValues(values);
                var item = new TReturnEntity();

                for (var i = 0; i < values.Length; i++)
                    columnsDictionary[selectFields[i]].Setter(item, values[i]);

                yield return item;
            }

            await reader.CloseAsync();
        }

        private static async Task<TempTable<TSelectEntity>> PopulateTempTable<TContext, TSelectEntity>(
            this TContext context,
            IAsyncEnumerable<TSelectEntity> list,
            CancellationToken cancellationToken)
                where TContext : DbContext
                where TSelectEntity : class
        {
            var tempTable = await context.CreateTempTable<TSelectEntity>(cancellationToken);

            await context.BulkInsertAsync(
                tempTable.TableName,
                list,
                cancellationToken
            );

            return tempTable;
        }

        private static async Task<(IDbContextTransaction transaction, bool isNewTransaction)>  GetTransaction<TContext>(
            this TContext context, 
            CancellationToken cancellationToken) 
                where TContext : DbContext
        {
            var transaction = GetCurrentTransaction(context);
            var isNewTransaction = false;

            if (transaction is null)
            {
                transaction = await BeginTransaction(context, cancellationToken);
                isNewTransaction = true;
            }

            return (transaction, isNewTransaction);
        }

        private static async Task<TempTable<TSelectEntity>> CreateTempTable<TSelectEntity>(
            this DbContext context,
            CancellationToken cancellationToken = default)
                where TSelectEntity : class
        {
            var type = typeof(TSelectEntity);
            var tmpTableName = $"{type.Name.ToLowerInvariant()}_{GetRandomTableSuffix(8)}";
            var fields = context.GetColumnNamesDictionary<TSelectEntity>(onlyEscaped: false);

            var connection = GetConnection(context);
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = $"""
                    CREATE TEMP TABLE {tmpTableName}
                    ({string.Join(", ", fields.Where(x => x.Key.StartsWith('"')).Select(x => $"{x.Key} {x.Value.PostgresType}"))})
                    ON COMMIT DROP
                    """;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return new TempTable<TSelectEntity>(tmpTableName);
        }

        internal static string GetMergeSql<TContext, TUpdateEntity, TSelectEntity>(
            this TContext context,
            IQueryable<TSelectEntity> toSelect,
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> predicate,
            Action<SqlActions<TUpdateEntity, TSelectEntity>> setActions,
            out LambdaToSql lambdaToSqlInstance)
                where TContext : DbContext
                where TUpdateEntity : class
                where TSelectEntity : class
        {
            var actions = new SqlActions<TUpdateEntity, TSelectEntity>();
            setActions(actions);

            if (actions.Count == 0)
                throw new InvalidOperationException("Can not process a merge with no sql actions.");

            const string UpdateAlias = "u";
            const string SelectAlias = "s";

            var updateType = typeof(TUpdateEntity);
            var selectType = typeof(TSelectEntity);

            var sb = new StringBuilder();

            var updateTableName = context.GetTableName<TUpdateEntity>();
            var selectStatement = toSelect switch
            {
                DbSet<TSelectEntity> dbSet => context.GetTableName(typeof(TSelectEntity)),
                TempTable<TSelectEntity> tempTable => tempTable.TableName,
                _ => $"({CustomEfCoreQueryCompiler.ToQueryString(context, toSelect)})"
            };

            var lambdaToSql = lambdaToSqlInstance = new LambdaToSql(
                context,
                new()
                {
                    [updateType] = UpdateAlias,
                    [selectType] = SelectAlias,
                }
            );

            var predicateSql = lambdaToSql.Translate(predicate);

            sb.AppendLine($"MERGE INTO {updateTableName} {UpdateAlias}");
            sb.AppendLine($"USING {selectStatement} {SelectAlias}");
            sb.AppendLine($"ON {predicateSql}");

            foreach (var action in actions)
            {
                sb.Append("WHEN");
                sb.Append(action.MatchType switch
                {
                    MatchType.OnMatch => " MATCHED",
                    MatchType.OnNotMatch => " NOT MATCHED",
                    MatchType.OnNotMatchBySource => " NOT MATCHED BY SOURCE",
                    _ => throw new NotSupportedException($"MatchType '{action.MatchType}' is not supported.")
                });

                if (action.Condition is not null)
                {
                    sb.Append(" AND ");
                    sb.Append(lambdaToSql.Translate(action.Condition));
                }

                sb.AppendLine(" THEN");

                if (action is UpdateOrInsertSqlAction<TUpdateEntity, TSelectEntity> updateOrInsertAction)
                {
                    switch (action.Type)
                    {
                        case SqlActionType.Insert:
                            sb.Append("  INSERT (");
                            sb.AppendJoin(", ", updateOrInsertAction.Fields.Select(x => lambdaToSql.Translate(x.Key, withAlias: false)));
                            sb.AppendLine(")");
                            sb.Append("  VALUES (");
                            sb.AppendJoin(", ", updateOrInsertAction.Fields.Select(x => lambdaToSql.Translate(x.Value)));
                            sb.AppendLine(")");
                            break;
                        case SqlActionType.Update:
                            sb.AppendLine("  UPDATE SET");
                            sb.Append("    ");
                            sb.AppendJoin($"{Environment.NewLine}    , ", updateOrInsertAction.Fields.Select(x => $"{lambdaToSql.Translate(x.Key, withAlias: false)} = {lambdaToSql.Translate(x.Value)}"));
                            sb.AppendLine();
                            break;
                        default:
                            throw new NotSupportedException($"ActionType '{action.Type}' is not supported when using an UpdateOrInsertSqlAction");
                    }
                }
                else
                {
                    switch (action.Type)
                    {
                        case SqlActionType.Delete:
                            sb.AppendLine("  DELETE");
                            break;
                        case SqlActionType.DoNothing:
                            sb.AppendLine("  DO NOTHING");
                            break;
                        default:
                            throw new NotSupportedException($"ActionType '{action.Type}' is not supported.");
                    }
                }
            }

            return sb.ToString();
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It is used for selecting update type.")]
        public static string GetMergeSql<TContext, TUpdateEntity, TSelectEntity, TReturnEntity>(
            this TContext context,
            Expression<Func<TContext, DbSet<TUpdateEntity>>> toUpdate,
            IQueryable<TSelectEntity> toSelect,
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> predicate,
            Action<SqlActions<TUpdateEntity, TSelectEntity>> setActions,
            Expression<Func<TUpdateEntity, TSelectEntity, TReturnEntity>> returning,
            out Dictionary<int, string> selectFields)
                where TContext : DbContext
                where TUpdateEntity : class
                where TSelectEntity : class
                where TReturnEntity : class
        {
            var mergeSql = context.GetMergeSql(
                toSelect,
                predicate,
                setActions,
                out var lambdaToSql
           );

            return $"""
                {mergeSql}
                RETURNING {GetReturnStatement(lambdaToSql, returning, out selectFields)}
                """;
        }

        private static string GetReturnStatement<TUpdateEntity, TSelectEntity, TReturnEntity>(
            LambdaToSql lambdaToSql,
            Expression<Func<TUpdateEntity, TSelectEntity, TReturnEntity>> returning,
            out Dictionary<int, string> selectFields)
                where TUpdateEntity : class
                where TSelectEntity : class
                where TReturnEntity : class
        {
            var sql = lambdaToSql.Translate(returning);
            selectFields = new(lambdaToSql.FieldMap);
            return sql;
        }

        internal static OrderedDictionary<string, ColumnInfo<TEntity>> GetColumnNamesDictionary<TEntity>(
            this DbContext context, 
            bool onlyEscaped)
                where TEntity : class =>
                    _columnNameCache.GetOrCreate($"{typeof(TEntity).FullName}_{onlyEscaped}", _ =>
                    {
                        var entityType = typeof(TEntity);

                        if (context.TryGetModel(entityType, out var model))
                            return model
                                .GetProperties()
                                .SelectMany(x =>
                                    onlyEscaped
                                        ? GetOnlyEscaped(x)
                                        : GetBothEscapedAndNotEscaped(x)
                                )
                                .ToOrderedDictionary(
                                    x => x.key,
                                    x => new ColumnInfo<TEntity>(
                                        GetPropertyGetter<TEntity>(x.propertyInfo), 
                                        GetPropertySetter<TEntity>(x.propertyInfo),
                                        GetPostgresTypeStrict(x.propertyInfo)
                                    )
                                );
                        else
                            return entityType
                                .GetProperties()
                                .SelectMany(x =>
                                    onlyEscaped
                                        ? GetOnlyEscaped(x)
                                        : GetBothEscapedAndNotEscaped(x)
                                )
                                .ToOrderedDictionary(
                                    x => x.key,
                                    x => new ColumnInfo<TEntity>(
                                        GetPropertyGetter<TEntity>(x.propertyInfo),
                                        GetPropertySetter<TEntity>(x.propertyInfo),
                                        GetPostgresTypeStrict(x.propertyInfo)
                                    )
                                );

                    }) ?? throw new InvalidOperationException("Can not get fields of model");

        private static IEnumerable<(string key, PropertyInfo propertyInfo)> GetBothEscapedAndNotEscaped(PropertyInfo info)
        {
            var (columnName, propertyInfo) = GetNotEscaped(info);

            yield return (columnName, propertyInfo);
            yield return (columnName.EscapePosgresToken(), propertyInfo);
        }

        private static IEnumerable<(string key, PropertyInfo propertyInfo)> GetOnlyEscaped(PropertyInfo info)
        {
            var (columnName, propertyInfo) = GetNotEscaped(info);

            yield return (columnName.EscapePosgresToken(), propertyInfo);
        }

        private static (string key, PropertyInfo propertyInfo) GetNotEscaped(PropertyInfo info)
        {
            string columnName;
            var attr = info.GetCustomAttribute<ColumnAttribute>();
            if (attr is not null && !string.IsNullOrWhiteSpace(attr.Name))
                columnName = attr.Name;
            else
                columnName = info.Name;

            return (columnName, info);
        }

        private static IEnumerable<(string key, PropertyInfo propertyInfo)> GetBothEscapedAndNotEscaped(IProperty property)
        {
            var (columnName, propertyInfo) = GetNotEscaped(property);

            yield return (columnName, propertyInfo);
            yield return (columnName.EscapePosgresToken(), propertyInfo);
        }

        private static IEnumerable<(string key, PropertyInfo propertyInfo)> GetOnlyEscaped(IProperty property)
        {
            var (columnName, propertyInfo) = GetNotEscaped(property);

            yield return (columnName.EscapePosgresToken(), propertyInfo);
        }

        private static (string key, PropertyInfo propertyInfo) GetNotEscaped(IProperty property)
        {
            var columnName = property.GetColumnName();

            return (columnName, property.PropertyInfo!);
        }

        private static string GetPostgresTypeStrict(MemberInfo? member) =>
            GetPostgresType(member, throwExceptionOnFailure: true)
                ?? throw new NotSupportedException($"postgres type can not be found.");

        internal static string? GetPostgresType(MemberInfo? member, bool throwExceptionOnFailure)
        {
            var returnType = member switch
            {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                MethodInfo methodInfo => methodInfo.ReturnType,
                _ => null
            };

            if (returnType is not null)
            {
                var underlyingType = Nullable.GetUnderlyingType(returnType);
                if (underlyingType is not null)
                    returnType = underlyingType;

                return Type.GetTypeCode(returnType) switch
                {
                    TypeCode.String or TypeCode.Char or TypeCode.Empty or TypeCode.DBNull => "text",
                    TypeCode.Boolean => "boolean",
                    TypeCode.DateTime => "timestamp",
                    TypeCode.Decimal => "numeric",
                    TypeCode.Double => "double precision",
                    TypeCode.Int16 or TypeCode.Byte or TypeCode.SByte => "smallint",
                    TypeCode.Int32 => "integer",
                    TypeCode.Int64 => "bigint",
                    TypeCode.Single => "real",
                    TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 =>
                         throwExceptionOnFailure
                            ? throw new NotSupportedException($"postgres does not support unsigned numbers. Runtime type '{returnType.FullName}' is not supported")
                            : null,
                    _ => throwExceptionOnFailure
                            ? throw new NotSupportedException($"postgres type for runtime type '{returnType.FullName}' is not supported")
                            : null
                };
            }
            else if (throwExceptionOnFailure)
                throw new NotSupportedException($"postgres type can not be found.");
            else
                return null;
        }

        private static Func<TEntity, object> GetPropertyGetter<TEntity>(PropertyInfo? propertyInfo) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);

            var parameterType = typeof(TEntity);
            var parameter = Expression.Parameter(parameterType, "x");
            var parameterAccess = Expression.Convert(Expression.MakeMemberAccess(parameter, propertyInfo), typeof(object));

            return Expression.Lambda<Func<TEntity, object>>(parameterAccess, parameter).Compile();
        }

        private static Action<TEntity, object> GetPropertySetter<TEntity>(PropertyInfo? propertyInfo) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);

            var parameterType = typeof(TEntity);
            var valueType = typeof(object);

            var parameterEntity = Expression.Parameter(parameterType, "x");
            var parameterValue = Expression.Parameter(valueType, "v");

            var entityProperty = Expression.MakeMemberAccess(parameterEntity, propertyInfo);

            Expression castValue;
            if (propertyInfo.PropertyType.IsEnum)
                castValue =
                    Expression.Convert(
                        Expression.Call(
                            ConvertValueToEnumMethodInfo,
                            [parameterValue, Expression.Constant(propertyInfo.PropertyType)]
                        ),
                        propertyInfo.PropertyType
                    );
            else
                castValue = Expression.Convert(parameterValue, propertyInfo.PropertyType);

            var assignment = Expression.Assign(entityProperty, castValue);

            return Expression.Lambda<Action<TEntity, object>>(assignment, parameterEntity, parameterValue).Compile();
        }

        private static readonly MethodInfo ConvertValueToEnumMethodInfo = typeof(DbContextExtensions)
            .GetMethod(nameof(ConvertValueToEnum), BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new ApplicationException($"Can not find method named '{nameof(ConvertValueToEnum)}'.");

        private static object ConvertValueToEnum(object enumValue, Type enumType) => enumValue switch
        {
            string str => Enum.Parse(enumType, str, ignoreCase: true),
            _ => Enum.ToObject(enumType, enumValue),
        };

        internal static string GetTableName<TEntity>(this DbContext context, bool withSchema = true)
            where TEntity : class =>
                context.GetTableName(typeof(TEntity), withSchema);

        internal static string GetTableName(this DbContext context, Type type, bool withSchema = true)
        {
            var model = context.GetModel(type);
            var tableName = model.GetTableName();

            if (!withSchema)
                return tableName.EscapePosgresToken();
            else
            {
                var schema = model.GetSchema();
                if (schema is null)
                    return tableName.EscapePosgresToken();
                else
                    return $"{schema.EscapePosgresToken()}.{tableName.EscapePosgresToken()}";
            }
        }

        private static string EscapePosgresToken(this string? token) =>
            $"\"{token?.Replace("\"", "\"\"")}\"";

        internal static IEntityType GetModel(this DbContext context, Type type) =>
            context.Model.FindEntityType(type)
                ?? throw new NotSupportedException($"Can not find model of '{type.FullName}'");

        internal static bool TryGetModel(this DbContext context, Type type, [NotNullWhen(true)] out IEntityType? entityType)
        {
            entityType = context.Model.FindEntityType(type);
            return entityType is not null;
        }

        internal record ColumnInfo<TEntity>(Func<TEntity, object> Getter, Action<TEntity, object> Setter, string PostgresType) { }

        internal class TempTable<TEntity>(string tableName) : IQueryable<TEntity>
            where TEntity : class
        {
            public string TableName => tableName;

            public Type ElementType => typeof(TEntity);

            public Expression Expression => throw new NotImplementedException();

            public IQueryProvider Provider => throw new NotImplementedException();

            public IEnumerator<TEntity> GetEnumerator() => throw new NotImplementedException();

            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        }
    }
}
