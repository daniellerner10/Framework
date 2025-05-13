namespace Keeper.Framework.Extensions.Collections;

public static class ArrayExtensions
{
    public static TSource[] ConcatAllArrays<TSource>(this TSource[][] lists)
    {
        if (lists.Length == 0)
            return [];
        else if (lists.Length == 1)
            return lists[0];
        else
        {
            var size = lists.Sum(x => x.Length);
            var newArr = new TSource[size];
            var position = 0;

            for (var i = 0; i < lists.Length; i++)
            {
                lists[i].CopyTo(newArr, position);
                position += lists[i].Length;
            }

            return newArr;
        }
    }

    public static int IndexOfArray<TSource>(this TSource[] arr, Func<TSource, bool> predicate)
    {
        for (var i = 0; i < arr.Length; i++)
        {
            if (predicate(arr[i]))
                return i;
        }

        return -1;
    }
}
