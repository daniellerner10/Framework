using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Keeper.Framework.Extensions.Reflection;

public static class TypeExtensions
{
    private readonly static Type EnumerableType = typeof(IEnumerable<>);

    public static IEnumerable<PropertyInfo> GetNonPublicProperties(this Type type) =>
        type.GetPropertiesInternal(BindingFlags.NonPublic | BindingFlags.Instance);

    public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type) =>
        type.GetPropertiesInternal(BindingFlags.Public | BindingFlags.Instance);

    public static IEnumerable<PropertyInfo> GetNonPublicStaticProperties(this Type type) =>
        type.GetPropertiesInternal(BindingFlags.NonPublic | BindingFlags.Static);

    public static IEnumerable<PropertyInfo> GetPublicStaticProperties(this Type type) =>
        type.GetPropertiesInternal(BindingFlags.Public | BindingFlags.Static);

    public static bool IsEnumerable(this Type type) => type.IsEnumerable(out _);

    public static bool IsEnumerable(this Type type, out Type enumeratedType)
    {
        var @interface = type.GetInterfaces().FirstOrDefault(static i => i.IsGenericType && i.GetGenericTypeDefinition() == EnumerableType);

        if (@interface is null)
        {
            enumeratedType = default!;
            return false;
        }

        enumeratedType = @interface.GetGenericArguments().First();
        return true;
    }

    internal static IEnumerable<PropertyInfo> GetPropertiesInternal(this Type type, BindingFlags bindingFlags)
    {
        if (type.IsInterface)
        {
            var properties = new List<PropertyInfo>();

            var visited = new HashSet<Type>();
            var queue = new Queue<Type>();

            visited.Add(type);
            queue.Enqueue(type);

            while (queue.Count > 0)
            {
                var subType = queue.Dequeue();
                foreach (var subInterface in subType.GetInterfaces().Where(i => visited.Add(i)))
                    queue.Enqueue(subInterface);

                var typeProperties = subType.GetProperties(BindingFlags.FlattenHierarchy | bindingFlags);

                var newProperties = typeProperties.Where(x => !properties.Contains(x));

                properties.AddRange(newProperties);
            }

            return properties;
        }
        else
            return type.GetProperties(BindingFlags.FlattenHierarchy | bindingFlags);
    }
}
