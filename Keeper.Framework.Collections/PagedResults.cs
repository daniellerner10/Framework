namespace Keeper.Framework.Collections;

/// <summary>
/// Paged results.
/// </summary>
/// <typeparam name="T">Type of object</typeparam>
public class PagedResults<T> : IPagedResults<T>
{
    /// <summary>
    /// `PagesResults<typeparamref name="T"/>` returns pages from an `Iqueryable<typeparamref name="T"/>` object. Use it as follows.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    public PagedResults(IQueryable<T> query, int pageNumber, int pageSize)
    {
        var list = query.Skip((pageNumber - 1) * pageSize).Take(pageSize + 1).ToList();
        Results = list.Take(pageSize).ToList();
        PageNumber = pageNumber;
        PageSize = Results.Count;
        HasPreviousPage = pageNumber > 1;
        HasNextPage = list.Count > pageSize;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public PagedResults()
    {
        Results = [];
    }

    /// <summary>
    /// The results.
    /// </summary>
    public List<T> Results { get; set; }

    /// <summary>
    /// The page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Has a previous page.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Has a next page.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// If you are returning items as entities, you may have to convert them to a contract. Do this with the `ConvertTo` method.
    /// `var contractResults = pagedResults.ConvertTo(e => new Contract(e));`
    /// You can generate the contract any way you want.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="converter"></param>
    /// <returns></returns>
    public PagedResults<U> ConvertTo<U>(Converter<T, U> converter)
    {
        return new PagedResults<U>()
        {
            PageNumber = PageNumber,
            PageSize = PageSize,
            HasPreviousPage = HasPreviousPage,
            HasNextPage = HasNextPage,
            Results = Results.ToList().ConvertAll(converter)
        };
    }

    IPagedResults<U> IPagedResults<T>.ConvertTo<U>(Converter<T, U> converter)
    {
        return ConvertTo(converter);
    }
}
