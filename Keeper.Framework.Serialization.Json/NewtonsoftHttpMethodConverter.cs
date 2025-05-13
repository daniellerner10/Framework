using Newtonsoft.Json;

namespace Keeper.Framework.Serialization.Json;

public class NewtonsoftHttpMethodConverter : JsonConverter<HttpMethod>
{
    public override HttpMethod? ReadJson(JsonReader reader, Type objectType, HttpMethod? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType != JsonToken.String)
            throw new JsonException($"The JSON value could not be converted to {typeof(HttpMethod)}.");

        try
        {
            return new HttpMethod((string)reader.Value!);
        }
        catch (Exception parseException)
        {
            throw new JsonException($"The JSON value '{reader.Value}' could not be converted to {typeof(HttpMethod)}.", parseException);
        }
    }

    public override void WriteJson(JsonWriter writer, HttpMethod? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.Method);
    }
}
