namespace Keeper.Framework.Extensions.Collections;

/// <summary>
/// Extension methods for lists.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Swaps two values in an array.
    /// </summary>
    /// <typeparam name="T">The type of element in the list.</typeparam>
    /// <param name="list">The list.</param>
    /// <param name="i1">The index of the first element to swap.</param>
    /// <param name="i2">The index of the second element to swap.</param>
    public static void Swap<T>(this IList<T> list, int i1, int i2)
    {
        (list[i2], list[i1]) = (list[i1], list[i2]);
    }

    /// <summary>
    /// Moves item in the list to end of list.
    /// </summary>
    /// <typeparam name="T">The type of item in the list</typeparam>
    /// <param name="list">The list.</param>
    /// <param name="index">The index of the item.</param>
    public static void MoveItemToEnd<T>(this IList<T> list, int index)
    {
        T item = list[index];
        list.RemoveAt(index);
        list.Add(item);
    }

    /// <summary>
    /// Moves item in the list to end of list.
    /// </summary>
    /// <typeparam name="T">The type of item in the list</typeparam>
    /// <param name="list">The list.</param>
    /// <param name="match">Predicate to match item in list.</param>
    public static void MoveItemToEnd<T>(this List<T> list, Predicate<T> match)
    {
        var index = list.FindIndex(match);
        list.MoveItemToEnd(index);
    }

    public static IEnumerable<IEnumerable<T>> ZipAll<T>(this IEnumerable<IEnumerable<T>> lists)
    {
        using var outerEnumerator = lists.GetEnumerator();

        if (outerEnumerator.MoveNext())
        {
            var firstEnumration = outerEnumerator.Current;

            if (outerEnumerator.MoveNext())
            {
                using var innerEnumerator = firstEnumration.GetEnumerator();

                while (innerEnumerator.MoveNext())
                {
                    foreach (var zipped in lists.Skip(1).ZipAll())
                    {
                        yield return Enumerable.Repeat(innerEnumerator.Current, 1)
                            .Concat(zipped);
                    }
                }
            }
            else
                foreach (var item in firstEnumration)
                    yield return Enumerable.Repeat(item, 1);
        }
    }
}
