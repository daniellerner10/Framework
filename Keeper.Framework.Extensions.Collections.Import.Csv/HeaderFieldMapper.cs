using System.Linq.Expressions;
using System.Reflection;

namespace Keeper.Framework.Extensions.Collections.Import.Csv;

internal class HeaderFieldMapper<TEntity> : IHeaderFieldMapper<TEntity>
{
    public IDictionary<string, PropertyInfo> HeaderPropertyMap { get; }

    public HeaderFieldMapper()
    {
        HeaderPropertyMap = new Dictionary<string, PropertyInfo>();
    }

    public IHeaderFieldMapper<TEntity> MapHeader<TResult>(string headerName, Expression<Func<TEntity, TResult>> selector)
    {
        if (selector.Body is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                HeaderPropertyMap.Add(headerName, propertyInfo);
                return this;
            }
            else
                throw new ArgumentException("Selecor body must return a property", nameof(selector));
        }
        else
            throw new ArgumentException("Selecor body must be a MemberExpression", nameof(selector));
    }
}