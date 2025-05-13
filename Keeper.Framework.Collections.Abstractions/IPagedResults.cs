namespace Keeper.Framework.Collections;

public interface IPagedResults<T>
{
    public List<T> Results { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public bool HasPreviousPage { get; set; }

    public bool HasNextPage { get; set; }

    /// <summary>
    /// If you are returning items as entities, you may have to convert them to a contract. Do this with the `ConvertTo` method.
    /// `var contractResults = pagedResults.ConvertTo(e => new Contract(e));`
    /// You can generate the contract any way you want.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="converter"></param>
    /// <returns></returns>
    public IPagedResults<U> ConvertTo<U>(Converter<T, U> converter);
}
