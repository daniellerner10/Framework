using System.Linq.Expressions;

namespace Keeper.Framework.Extensions.Collections.Import.Csv;

/// <summary>
/// Mapper used to map indexes in the csv to fields in the <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity to map.</typeparam>
public interface IHeaderFieldMapper<TEntity>
{
    /// <summary>
    /// Maps a header to a field of <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type of the selector</typeparam>
    /// <param name="headerName">The header name to map.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The mapper for fluent syntax.</returns>
    IHeaderFieldMapper<TEntity> MapHeader<TResult>(string headerName, Expression<Func<TEntity, TResult>> selector);
}