using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Keeper.Framework.Validations;

/// <summary>
///  In order to allow validations to run on enums,
///  the following is necessary to prevent the json reader from throwing an exception before reaching the validations.
///
/// Note: TO BE USED WITH NULLABLE ENUM!
/// </summary>
public class CustomStringEnumConverter : StringEnumConverter
{
    /// <summary>
    /// Reads the json and returns an object.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="objectType">The object type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The serializer.</param>
    /// <returns>The object</returns>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch (JsonSerializationException)
        {
            return null;
        }
    }
}
