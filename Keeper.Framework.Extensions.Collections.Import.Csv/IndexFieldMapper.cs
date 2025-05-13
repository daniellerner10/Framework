using System.Linq.Expressions;
using System.Reflection;

namespace Keeper.Framework.Extensions.Collections.Import.Csv;

internal class IndexFieldMapper<TEntity> : IIndexFieldMapper<TEntity>
{
    public IDictionary<int, PropertyInfo> IndexPropertyMap { get; }

    public IndexFieldMapper()
    {
        IndexPropertyMap = new Dictionary<int, PropertyInfo>();
    }

    public IIndexFieldMapper<TEntity> MapIndex<TResult>(int index, Expression<Func<TEntity, TResult>> selector)
    {
        if (selector.Body is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                IndexPropertyMap.Add(index, propertyInfo);
                return this;
            }
            else
                throw new ArgumentException("Selecor body must return a property", nameof(selector));
        }
        else
            throw new ArgumentException("Selecor body must be a MemberExpression", nameof(selector));
    }
}