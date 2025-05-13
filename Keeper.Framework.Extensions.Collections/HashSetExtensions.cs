namespace Keeper.Framework.Extensions.Collections;

public static class HashSetExtensions
{
    public static HashSet<T> MergeHashSets<T>(this HashSet<T> first, HashSet<T> second) =>
        new(first.Concat(second));
}
