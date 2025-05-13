using System.Reflection;
using Keeper.Framework.Collections;
using Newtonsoft.Json;

namespace Keeper.Masking
{
    public static class MaskJsonHelper
    {
        private static readonly SynchronizedDictionary<Type, JsonMasker> _cache = new();

        public static string ApplyMaskToJson(this Type type, string json) =>
            _cache.GetOrAdd(type, t =>
            {
                var properties = GetAllPropertiesToMask(t).ToList();
                if (properties.Count > 0)
                {
                    var jsonMasker = new JsonMasker(properties);

                    return jsonMasker;
                }
                else
                    return JsonMasker.NullMasker;

            }).Mask(json);

        private static IEnumerable<PropertyToMask> GetAllPropertiesToMask(Type t) =>
            GetAllPropertiesToMaskInternal(t).Distinct();

        private static IEnumerable<PropertyToMask> GetAllPropertiesToMaskInternal(Type t, int depth = 0)
        {
            if (depth < 3)
            {
                var properties = t.GetProperties();

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        var maskAttribute = property.GetCustomAttribute<MaskAttribute>(inherit: true);
                        if (maskAttribute is not null)
                        {
                            var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                            var propertyJsonName = jsonPropertyAttribute?.PropertyName ?? property.Name;

                            yield return new(propertyJsonName, maskAttribute);
                        }
                    }
                    else if (!property.PropertyType.IsValueType)
                    {
                        foreach (var tuple in GetAllPropertiesToMaskInternal(property.PropertyType, depth + 1))
                            yield return tuple;
                    }
                }
            }
        }
    }
}
