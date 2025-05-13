using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keeper.Framework.Serialization.Json
{
    public static class JsonConvertExtensions
    {
        public static IEnumerable<T> ReadJsonArrayFromStream<T>(
            this Stream stream,
            bool leaveOpen = true,
            JsonSerializerSettings? settings = default)
        {
            using var sr = new StreamReader(stream, leaveOpen: leaveOpen);
            using var reader = new JsonTextReader(sr);

            foreach(var item in reader.ReadJsonArrayFromJsonTextReader<T>(settings))
                yield return item;
        }

        public static IEnumerable<T> ReadJsonArrayFromJsonTextReader<T>(
            this JsonTextReader reader,
            JsonSerializerSettings? settings = default)
        {
            var jsonSerializer = JsonSerializer.CreateDefault(settings);

            bool isArray = reader.TokenType == JsonToken.StartArray;
            while (!isArray && reader.Read())
                isArray = reader.TokenType == JsonToken.StartArray;

            if (!isArray)
                throw new InvalidOperationException("Json in stream must be an array.");

            while (reader.Read())
                if (reader.TokenType == JsonToken.StartObject)
                    yield return jsonSerializer.Deserialize<T>(reader) ?? throw new NullReferenceException("Json deserialization returned unexpected null value.");
                else if (reader.TokenType == JsonToken.EndArray)
                    break;
        }

        public static async IAsyncEnumerable<T> ReadJsonArrayFromStreamAsync<T>(
            this Stream stream,
            bool leaveOpen = true,
            JsonSerializerSettings? settings = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var sr = new StreamReader(stream, leaveOpen: leaveOpen);
            using var reader = new JsonTextReader(sr);

            await foreach (var item in reader.ReadJsonArrayFromJsonTextReaderAsync<T>(settings, cancellationToken))
                yield return item;
        }

        public static async IAsyncEnumerable<T> ReadJsonArrayFromJsonTextReaderAsync<T>(
            this JsonTextReader reader,
            JsonSerializerSettings? settings = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var jsonSerializer = JsonSerializer.CreateDefault(settings);

            bool isArray = reader.TokenType == JsonToken.StartArray;
            while (!isArray && reader.Read())
                isArray = reader.TokenType == JsonToken.StartArray;

            if (!isArray)
                throw new InvalidOperationException("Json in stream must be an array.");

            while (await reader.ReadAsync(cancellationToken))
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var token = await JToken.LoadAsync(reader, cancellationToken);
                    yield return token.ToObject<T>(jsonSerializer) ?? throw new NullReferenceException("Json deserialization returned unexpected null value.");
                }
                else if (reader.TokenType == JsonToken.EndArray)
                    break;
        }
    }
}
