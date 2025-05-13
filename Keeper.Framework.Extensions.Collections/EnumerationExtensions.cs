using System.ComponentModel;

namespace Keeper.Framework.Extensions.Collections;

public static class EnumerationExtensions
{
    private static readonly Dictionary<Type, Dictionary<object, string>> _enumTypeMemberDescriptionCache = [];

    public static string? GetDescription<TEnum>(this TEnum enumMember) where TEnum : notnull
    {
        if (enumMember is null)
            return null;

        var enumType = typeof(TEnum);

        ref var typeDictionary = ref _enumTypeMemberDescriptionCache.GetValueRefOrAddDefault(enumType, out var exists)!;
        if (!exists)
        {
            lock (_enumTypeMemberDescriptionCache)
                typeDictionary = GetEnumMembersDescriptionValues(enumType);
        }

        var enumMemberName = enumMember.ToString()!;
        if (typeDictionary.TryGetValue(enumMemberName, out var res))
            return res;

        return null;
    }

    public static void PreCacheEnumDescriptions(params Type[] scanMarkers)
    {
        foreach (var scanMarker in scanMarkers)
        {
            var enumTypes = scanMarker
                .Assembly
                .ExportedTypes
                .Where(x => x.IsEnum);

            CacheEnumExportTypes(enumTypes);
        }
    }

    public static Dictionary<object, string> GetEnumMembersDescriptionValues(Type enumType)
    {
        return enumType
           .GetFields()
           .Select(x => (x.Name, x.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault()))
           .ToDictionary(x => (object)x.Name, x => x.Item2?.GetDescription() ?? x.Name);
    }

    private static void CacheEnumExportTypes(IEnumerable<Type> enumTypes)
    {
        foreach (var enumType in enumTypes)
        {
            if (!enumType.IsEnum)
                continue;

            ref var typeDictionary = ref _enumTypeMemberDescriptionCache.GetValueRefOrAddDefault(enumType, out var exists)!;
            if (!exists)
            {
                lock (_enumTypeMemberDescriptionCache)
                    typeDictionary = GetEnumMembersDescriptionValues(enumType);
            }
        }
    }
}
