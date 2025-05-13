using System.Collections;
using System.Linq.Expressions;

namespace Keeper.Framework.Extensions.Data
{
    public class SqlActions<TUpdateEntity, TSelectEntity> : IReadOnlyCollection<SqlAction<TUpdateEntity, TSelectEntity>>
        where TUpdateEntity : class
        where TSelectEntity : class
    {
        internal SqlActions() { }

        private List<SqlAction<TUpdateEntity, TSelectEntity>> Actions { get; } = [];

        public int Count => Actions.Count;

        public IEnumerator<SqlAction<TUpdateEntity, TSelectEntity>> GetEnumerator() => Actions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly HashSet<MatchType> _unconditionedMatchTypes = [];

        public SqlActions<TUpdateEntity, TSelectEntity> Insert(
            Dictionary<Expression<Func<TUpdateEntity, object?>>, Expression<Func<TUpdateEntity, TSelectEntity, object?>>> fields)
        {
            var action = new InsertSqlAction<TUpdateEntity, TSelectEntity>()
            {
                Fields = new(fields)
            };

            if (!_unconditionedMatchTypes.Add(action.MatchType))
                throw new NotSupportedException($"Can only add one unconditioned action of type '{action.MatchType}'");

            Actions.Add(action);

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Insert(
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> condition,
            Dictionary<Expression<Func<TUpdateEntity, object?>>, Expression<Func<TUpdateEntity, TSelectEntity, object?>>> fields)
        {
            Actions.Add(new InsertSqlAction<TUpdateEntity, TSelectEntity>(condition)
            {
                Fields = new(fields)
            });

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Update(
            Dictionary<Expression<Func<TUpdateEntity, object?>>, Expression<Func<TUpdateEntity, TSelectEntity, object?>>> fields)
        {
            var action = new UpdateSqlAction<TUpdateEntity, TSelectEntity>()
            {
                Fields = new(fields)
            };

            if (!_unconditionedMatchTypes.Add(action.MatchType))
                throw new NotSupportedException($"Can only add one unconditioned action of type '{action.MatchType}'");

            Actions.Add(action);

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Update(
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> condition,
            Dictionary<Expression<Func<TUpdateEntity, object?>>, Expression<Func<TUpdateEntity, TSelectEntity, object?>>> fields)
        {
            Actions.Add(new UpdateSqlAction<TUpdateEntity, TSelectEntity>(condition)
            {
                Fields = new(fields)
            });

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Update(
            MatchType matchType,
            Dictionary<Expression<Func<TUpdateEntity, object?>>, Expression<Func<TUpdateEntity, TSelectEntity, object?>>> fields)
        {
            var action = new UpdateSqlAction<TUpdateEntity, TSelectEntity>(matchType: matchType)
            {
                Fields = new(fields)
            };

            if (!_unconditionedMatchTypes.Add(action.MatchType))
                throw new NotSupportedException($"Can only add one unconditioned action of type '{action.MatchType}'");

            Actions.Add(action);

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Update(
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> condition,
            MatchType matchType,
            Dictionary<Expression<Func<TUpdateEntity, object?>>, Expression<Func<TUpdateEntity, TSelectEntity, object?>>> fields)
        {
            Actions.Add(new UpdateSqlAction<TUpdateEntity, TSelectEntity>(condition, matchType)
            {
                Fields = new(fields)
            });

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Delete()
        {
            var action = new DeleteSqlAction<TUpdateEntity, TSelectEntity>();

            if (!_unconditionedMatchTypes.Add(action.MatchType))
                throw new NotSupportedException($"Can only add one unconditioned action of type '{action.MatchType}'");

            Actions.Add(action);

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Delete(Expression<Func<TUpdateEntity, TSelectEntity, bool>> condition)
        {
            Actions.Add(new DeleteSqlAction<TUpdateEntity, TSelectEntity>(condition));

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Delete(MatchType matchType)
        {
            var action = new DeleteSqlAction<TUpdateEntity, TSelectEntity>(matchType: matchType);
            Actions.Add(action);

            if (!_unconditionedMatchTypes.Add(action.MatchType))
                throw new NotSupportedException($"Can only add one unconditioned action of type '{action.MatchType}'");

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> Delete(
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> condition,
            MatchType matchType)
        {
            Actions.Add(new DeleteSqlAction<TUpdateEntity, TSelectEntity>(condition, matchType));

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> DoNothing()
        {
            var action = new DoNothingSqlAction<TUpdateEntity, TSelectEntity>();

            if (!_unconditionedMatchTypes.Add(action.MatchType))
                throw new NotSupportedException($"Can only add one unconditioned action of type '{action.MatchType}'");

            Actions.Add(action);

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> DoNothing(Expression<Func<TUpdateEntity, TSelectEntity, bool>> condition)
        {
            Actions.Add(new DoNothingSqlAction<TUpdateEntity, TSelectEntity>(condition));

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> DoNothing(MatchType matchType)
        {
            var action = new DoNothingSqlAction<TUpdateEntity, TSelectEntity>(matchType: matchType);
            Actions.Add(action);

            if (!_unconditionedMatchTypes.Add(action.MatchType))
                throw new NotSupportedException($"Can only add one unconditioned action of type '{action.MatchType}'");

            return this;
        }

        public SqlActions<TUpdateEntity, TSelectEntity> DoNothing(
            Expression<Func<TUpdateEntity, TSelectEntity, bool>> condition,
            MatchType matchType)
        {
            Actions.Add(new DoNothingSqlAction<TUpdateEntity, TSelectEntity>(condition, matchType));

            return this;
        }
    }

    public abstract class SqlAction<TUpdateEntity, TSelectEntity>(
        Expression<Func<TUpdateEntity, TSelectEntity, bool>>? condition, 
        SqlActionType type, 
        MatchType matchType)
            where TUpdateEntity : class
            where TSelectEntity : class
    {
        public SqlActionType Type => type;

        public MatchType MatchType => matchType;

        public Expression<Func<TUpdateEntity, TSelectEntity, bool>>? Condition => condition;
    }

    public abstract class UpdateOrInsertSqlAction<TUpdateEntity, TSelectEntity>(
        Expression<Func<TUpdateEntity, TSelectEntity, bool>>? condition,
        SqlActionType type,
        MatchType matchType)
            : SqlAction<TUpdateEntity, TSelectEntity>(condition, type, matchType)
                  where TUpdateEntity : class
                  where TSelectEntity : class
    {
        public required Dictionary<Expression<Func<TUpdateEntity, object?>>, Expression<Func<TUpdateEntity, TSelectEntity, object?>>> Fields { get; init; }
    }

    internal class InsertSqlAction<TUpdateEntity, TSelectEntity>(
        Expression<Func<TUpdateEntity, TSelectEntity, bool>>? condition = default) :
            UpdateOrInsertSqlAction<TUpdateEntity, TSelectEntity>(condition, SqlActionType.Insert, MatchType.OnNotMatch)
                where TUpdateEntity : class
                where TSelectEntity : class { }

    internal class UpdateSqlAction<TUpdateEntity, TSelectEntity> :
        UpdateOrInsertSqlAction<TUpdateEntity, TSelectEntity>
            where TUpdateEntity : class
            where TSelectEntity : class
    {
        public UpdateSqlAction(
            Expression<Func<TUpdateEntity, TSelectEntity, bool>>? condition = default,
            MatchType matchType = MatchType.OnMatch) : base(condition, SqlActionType.Update, matchType)
        {
            if (matchType == MatchType.OnNotMatch)
                throw new ArgumentException("MatchType.OnNotMatch is not supported for SqlActionType.Update");
        }
    }

    internal class DeleteSqlAction<TUpdateEntity, TSelectEntity> :
        SqlAction<TUpdateEntity, TSelectEntity>
            where TUpdateEntity : class
            where TSelectEntity : class
    {
        public DeleteSqlAction(
            Expression<Func<TUpdateEntity, TSelectEntity, bool>>? condition = default,
            MatchType matchType = MatchType.OnMatch) : base(condition, SqlActionType.Delete, matchType)
        {
            if (matchType == MatchType.OnNotMatch)
                throw new ArgumentException("MatchType.OnNotMatch is not supported for SqlActionType.Delete");
        }
    }

    internal class DoNothingSqlAction<TUpdateEntity, TSelectEntity>(
        Expression<Func<TUpdateEntity, TSelectEntity, bool>>? condition = default,
        MatchType matchType = MatchType.OnMatch) :
            SqlAction<TUpdateEntity, TSelectEntity>(condition, SqlActionType.DoNothing, matchType)
                where TUpdateEntity : class
                where TSelectEntity : class { }
}
