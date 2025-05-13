using Keeper.Framework.Extensions.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Keeper.Framework.Extensions.Collections;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ObjectAsEnumerable<T>(this T obj)
    {
        yield return obj;
    }

    public static IEnumerable<T> ConcatItemIf<T>(this IEnumerable<T> list, T obj, Func<bool> ifPredicate)
    {
        if (!ifPredicate())
            return list;

        return list.ConcatItem(obj);
    }

    public static IEnumerable<T> ConcatItem<T>(this IEnumerable<T> list, T obj)
    {
        foreach (var item in list)
            yield return item;

        yield return obj;
    }

    public static IEnumerable<T> ExceptItem<T>(this IEnumerable<T> list, T obj)
        where T : IEquatable<T>
    {
        foreach (var item in list)
        {
            if (item is null)
            {
                if (obj is not null)
                    yield return item!;
            }
            else if (!item!.Equals(obj))
                yield return item;
        }
    }

    public static IEnumerable<T> ConcatNonNullItem<T>(this IEnumerable<T> list, T? obj)
    {
        foreach (var item in list)
            yield return item;

        if (obj is not null)
            yield return obj;
    }

    /// <summary>
    /// Run action per item on the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        if (collection != null)
            foreach (var item in collection)
                action(item);
    }

    /// <summary>
    /// Run an async action per item on the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static Task ForEachAsync<T>(this IEnumerable<T> source, Action<T> body, CancellationToken cancellationToken = default) =>
        Task.WhenAll(
            from item in source
            select Task.Run(() => body(item), cancellationToken));

    /// <summary>
    /// Run an async action per item on the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body) =>
        Task.WhenAll(source.Select(s => body(s)));

    /// <summary>
    /// Serialize any items. Examber ['item1','item2',...]
    /// </summary>
    public static string Serialize<T>(this IEnumerable<T> arr) =>
        $@"['{string.Join("','", arr)}']";

    /// <summary>
    /// Returns true if the collection has duplicates, otherwise false.
    /// </summary>
    public static bool HasDuplicates<T>(this IEnumerable<T> list)
    {
        var hashset = new HashSet<T>();
        return !list?.All(hashset.Add) ?? false;
    }

    /// <summary>
    /// Performs an linq Any method with an async predicate.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="list">The list.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Whether any of the items match the predicate.</returns>
    public static async Task<bool> AnyAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> predicate)
    {
        foreach (var item in list)
            if (await predicate(item))
                return true;

        return false;
    }

    /// <summary>
    /// An async implementation of linq's where.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="list">The list.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The filtered enumeration.</returns>
    public static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> predicate)
    {
        var result = new List<T>();

        foreach (var item in list)
            if (await predicate(item))
                result.Add(item);

        return result;
    }

    /// <summary>
    /// Returns true if 1) the enumerable is null or 2) is empty after applying the predicate method to each item. Otherwise, returns false.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static bool NullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? list, Func<T, bool>? predicate = null) =>
        list is null || (predicate is null ? !list.Any() : !list.Any(predicate));

    /// <summary>
    /// Returns the list if the list is not null. Otherwise, empty collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? list) =>
        list ?? [];

    public static IEnumerable<T> IntersperseNonNull<T>(this IEnumerable<T> list, T? item) =>
        item is null ? list : list.Intersperse(item);

    public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> list, T item)
    {
        var first = true;
        using var enumerator = list.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (!first)
                yield return item;

            first = false;
            yield return enumerator.Current;
        }
    }

    public static IEnumerable<(T1 First, T2 Second)> Permutations<T1, T2>(this IEnumerable<T1> list1, IEnumerable<T2> list2)
    {
        using var enumerator1 = list1.GetEnumerator();
        while (enumerator1.MoveNext())
        {
            using var enumerator2 = list2.GetEnumerator();
            while (enumerator2.MoveNext())
                yield return (enumerator1.Current, enumerator2.Current);
        }
    }

    public static bool AllSame<T>(this IEnumerable<T> enumerable)
    {
        using var enumerator = enumerable.GetEnumerator();

        if (enumerator.MoveNext())
        {
            var firstValue = enumerator.Current;
            while (enumerator.MoveNext())
                if (firstValue is null)
                {
                    if (enumerator.Current is not null)
                        return false;
                }
                else
                {
                    if (enumerator.Current is null || !enumerator.Current.Equals(firstValue))
                        return false;
                }
        }

        return true;
    }

    /// <summary>
    /// Flatten an enumerable with objects that contain a hierarchy.
    /// </summary>
    /// <typeparam name="T">The type in the enumeration.</typeparam>
    /// <param name="list">The list to flatten</param>
    /// <param name="nextLevel">A method used to get the next level.</param>
    /// <returns>The flattened enumerable.</returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> nextLevel)
    {
        var flatList = new List<T>();
        Flatten(flatList, list, nextLevel);
        return flatList;
    }

    private static void Flatten<T>(List<T> flatList, IEnumerable<T> currentLevel, Func<T, IEnumerable<T>> nextLevel)
    {
        if (currentLevel is not null)
        {
            flatList.AddRange(currentLevel);

            foreach (var item in currentLevel)
                Flatten(flatList, nextLevel(item), nextLevel);
        }
    }

    public static IEnumerable<TSource> IntersectIfAny<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
        if (first.Any())
            if (second.Any())
                return first.Intersect(second);
            else
                return first;
        else
            return second;
    }

    public static IEnumerable<TSource> ConcatAll<TSource>(this IEnumerable<IEnumerable<TSource>> lists)
    {
        foreach (var list in lists)
            foreach (var item in list)
                yield return item;
    }

    public static IEnumerable<T> ConcatItems<T>(this T item, params T[] others)
    {
        yield return item;

        foreach (var other in others)
            yield return other;
    }

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> list, Func<bool> condition, Func<T, bool> predicate)
    {
        if (condition())
            return list.Where(predicate);
        else
            return list;
    }

    public static bool ContainsAnyOf<T>(this IEnumerable<T> list, IEnumerable<T> filter)
    {
        if (filter is not HashSet<T> hash)
            hash = filter.ToHashSet();

        return list.Any(hash.Contains);
    }

    public static IEnumerable<T> WhereAnyOf<T>(this IEnumerable<T> list, IEnumerable<T> filter)
    {
        if (filter is not HashSet<T> hash)
            hash = filter.ToHashSet();

        return list.Where(hash.Contains);
    }

    public static IEnumerable<T> FirstWhere<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        using var enumerator = list.GetEnumerator();

        while (enumerator.MoveNext())
        {
            if (predicate(enumerator.Current))
            {
                yield return enumerator.Current;
                break;
            }
        }
    }

    public static IEnumerable<T> UniqueSet<T>(this IEnumerable<IEnumerable<T>> list)
        where T : notnull
    {
        using var enumerator = list.GetEnumerator();

        enumerator.MoveNext();

        var first = enumerator.Current;

        while (enumerator.MoveNext())
            if (!first.IsUniqueSetWith(enumerator.Current))
                throw new InvalidOperationException("list is not a unique set.");

        return first;
    }

    public static bool IsUniqueSetWith<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        where T : notnull
    {
        var valueMap = list1.GroupBy(x => x)
                            .ToDictionary(x => x.Key, x => x.Count());

        foreach (var item in list2)
        {
            if (valueMap.TryGetValue(item, out var counter))
            {
                if (counter == 0)
                    return false;

                counter--;
            }
            else
                return false;
        }

        return true;
    }

    public static IEnumerable<string> Distinct(this IEnumerable<string> list, bool caseInsensitive)
    {
        if (caseInsensitive)
            return list.Distinct(CaseInsensitiveEqualityComparer.Instance);
        else
            return list.Distinct();
    }

    public static IEnumerable<TResult> SelectPairs<TSource, TResult>(this IEnumerable<TSource> list, Func<TSource, TSource?, TResult> selector)
    {
        using var enumerator = list.GetEnumerator();

        TSource first;
        TSource? second;

        while (true)
        {
            if (enumerator.MoveNext())
            {
                first = enumerator.Current;

                if (enumerator.MoveNext())
                    second = enumerator.Current;
                else
                {
                    yield return selector(first, default);
                    break;
                }

                yield return selector(first, second);
            }
            else
                break;
        }
    }

    public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }

    public static IEnumerable<object> ToEnumerable(this System.Collections.IEnumerator enumerator)
    {
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }

    public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        var index = -1;
        foreach (var item in list)
        {
            index++;
            if (predicate(item))
                return index;
        }
        return -1;
    }

    private class CaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) =>
            x?.Equals(y, StringComparison.InvariantCultureIgnoreCase) ?? y is null;

        public int GetHashCode([DisallowNull] string obj) =>
            obj.ToLowerInvariant().GetHashCode();

        public static readonly CaseInsensitiveEqualityComparer Instance = new();
    }
}
