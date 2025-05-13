using Keeper.Framework.Extensions.Reflection;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Keeper.Framework.Extensions.Collections.Import.Csv;

/// <summary>
/// Extensions for exporting collections.
/// </summary>
public static partial class ImportCollectionsExtensionsCsv
{
    private const string FieldValueGroupName = "v";

    [GeneratedRegex($@"(?<=,|^)(?:\s*?""(?<{FieldValueGroupName}>(?:[^""]|"""")*)""|(?<{FieldValueGroupName}>[^,]*))")]
    private static partial Regex CsvParseFieldsRegex();

    /// <summary>
    /// Converts a stream of csv data to an enumeration of entities.  This assumes that there is a header
    /// row and that the header names match the property names of the <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to create.</typeparam>
    /// <param name="stream">The stream with the csv data.</param>
    /// <returns>An enumeration of entities</returns>
    public static IEnumerable<TEntity> FromCsvStream<TEntity>(this Stream stream)
        where TEntity : class, new() =>
        stream.FromCsvStream<TEntity, string>(default, hasHeaders: true);

    /// <summary>
    /// Converts a stream of csv data to an enumeration of entities.  You must use the <paramref name="mapperBuilder"/>
    /// to map header names to property names of the <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to create.</typeparam>
    /// <param name="stream">The stream with the csv data.</param>
    /// <param name="mapperBuilder">A callback that receives a mapper which is used to map headers to fields.</param>
    /// <returns>An enumeration of entities</returns>
    public static IEnumerable<TEntity> FromCsvStream<TEntity>(this Stream stream, Action<IHeaderFieldMapper<TEntity>> mapperBuilder)
        where TEntity : class, new()
    {
        var mapper = new HeaderFieldMapper<TEntity>();
        mapperBuilder(mapper);
        return stream.FromCsvStream<TEntity, string>(mapper.HeaderPropertyMap, hasHeaders: true);
    }

    /// <summary>
    /// Converts a stream of csv data to an enumeration of entities.  You must use the <paramref name="mapperBuilder"/>
    /// to map column indexes to property names of the <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to create.</typeparam>
    /// <param name="stream">The stream with the csv data.</param>
    /// <param name="mapperBuilder">A callback that receives a mapper which is used to map indexes to fields.</param>
    /// <param name="hasHeaders">True if the data has headers.  In such a case, we skip the first row.</param>
    /// <returns>An enumeration of entities</returns>
    public static IEnumerable<TEntity> FromCsvStream<TEntity>(this Stream stream, Action<IIndexFieldMapper<TEntity>> mapperBuilder, bool hasHeaders)
        where TEntity : class, new()
    {
        var mapper = new IndexFieldMapper<TEntity>();
        mapperBuilder(mapper);
        return stream.FromCsvStream<TEntity, int>(mapper.IndexPropertyMap, hasHeaders);
    }

    private static IEnumerable<TEntity> FromCsvStream<TEntity, TMapKey>(this Stream stream, IDictionary<TMapKey, PropertyInfo>? propertyMap, bool hasHeaders = true)
        where TEntity : class, new()
    {
        var isStringKey = typeof(TMapKey).Equals(typeof(string));

        if (!hasHeaders && (isStringKey || propertyMap is null))
            throw new ArgumentException("If there are no headers, then you must use a column index based propertyMap");

        var reader = new StreamReader(stream);

        Dictionary<int, string>? indexToHeadersMap = default;

        if (hasHeaders)
            indexToHeadersMap = CsvParseFieldsRegex().Matches(reader!.ReadLine()!)
                                                     .AsEnumerable()
                                                     .Select(static (m, i) => new { Match = m, Index = i })
                                                     .ToDictionary(static x => x.Index, static x => x.Match.Groups[FieldValueGroupName].Value.DecodeCsvField());

        propertyMap ??= (IDictionary<TMapKey, PropertyInfo>)BuildPropertyMap<TEntity>(indexToHeadersMap!);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()!;
            var fields = CsvParseFieldsRegex().Matches(line)
                                              .AsEnumerable()
                                              .Select((m, i) => new { Value = m.Groups[FieldValueGroupName].Value.DecodeCsvField(), Index = i });

            var entity = new TEntity();

            foreach (var field in fields)
            {
                object key = isStringKey ? indexToHeadersMap![field.Index] : field.Index;

                if (propertyMap.TryGetValue((TMapKey)key, out var prop))
                {
                    var val = ParseValue(prop, field.Value);
                    prop.SetValue(entity, val);
                }
            }

            yield return entity;
        }
    }

    private static object ParseValue(PropertyInfo prop, string str) =>
        TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(str)!;

    private static Dictionary<string, PropertyInfo> BuildPropertyMap<TEntity>(IDictionary<int, string> indexToHeadersMap)
    {
        var entityType = typeof(TEntity);
        var propertyMap = new Dictionary<string, PropertyInfo>();
        var properties = entityType.GetPublicProperties();

        foreach (var header in indexToHeadersMap.Values)
        {
            var prop = properties.FirstOrDefault(
                                    p =>
                                    {
                                        if (p.Name == header) return true;

                                        var att = p.GetCustomAttribute<DisplayNameAttribute>();
                                        return att?.DisplayName == header;
                                    });

            if (prop is not null)
                propertyMap.Add(header, prop);
        }

        return propertyMap;
    }

    private static string DecodeCsvField(this string field) =>
        field.Replace("\"\"", "\"");
}
