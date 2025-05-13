namespace Keeper.Framework.Extensions.Collections;

public static class List
{
    public static List<T> Empty<T>()
         => EmptyList<T>.Value;
}

internal static class EmptyList<T>
{
    public static readonly List<T> Value = [];
}
