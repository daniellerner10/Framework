using System.Linq.Expressions;

namespace Keeper.Framework.Extensions.Collections.Import.Csv;

/// <summary>
/// Maps an index to a field of <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity to map.</typeparam>
public interface IIndexFieldMapper<TEntity>
{
    /// <summary>
    /// Maps a index to a field of <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type of the selector</typeparam>
    /// <param name="index">The zero based index to map.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The mapper for fluent syntax.</returns>
    IIndexFieldMapper<TEntity> MapIndex<TResult>(int index, Expression<Func<TEntity, TResult>> selector);
}