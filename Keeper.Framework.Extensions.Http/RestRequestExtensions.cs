using System.Collections.Specialized;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keeper.Framework.Extensions.Http;

/// <summary>
/// Extensions for rest requests.
/// </summary>
public static class RestRequestExtensions
{
    /// <summary>
    /// Apply object as query string
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="obj">The object.</param>
    public static void ApplyObjectAsQueryString(this IRestRequest request, object obj)
    {
        var builder = new UriBuilder(request.Resource);

        var oldQuery = HttpUtility.ParseQueryString(builder.Query);
        var newQuery = HttpUtility.ParseQueryString(obj.ToQueryString());

        oldQuery.Add(newQuery);

        builder.Query = oldQuery.ToString();

        request.Resource = builder.Uri.ToString();
    }

    /// <summary>
    /// True if http method allows payload in body.  False otherwise.
    /// </summary>
    /// <param name="method">The http method</param>
    /// <returns>true if payload allowed in body.</returns>
    public static bool AllowPayloadInBody(this HttpMethod method) => method != HttpMethod.Get && method != HttpMethod.Delete;

    /// <summary>
    /// Converts an object to a valid query string.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>The query string.</returns>
    public static string ToQueryString(this object obj)
    {
        if (obj == null)
            return string.Empty;

        var json = JsonConvert.SerializeObject(obj);
        var jToken = JToken.Parse(json);
        var pairs = new NameValueCollection();

        AddPairsToToken(jToken, ref pairs);

        return $"?{pairs.ToQueryString()}";
    }

    /// <summary>
    /// Converts a <see cref="NameValueCollection"/> to a valid query string.
    /// </summary>
    /// <param name="nvc">The <see cref="NameValueCollection"/>.</param>
    /// <returns>The query string.</returns>
    public static string ToQueryString(this NameValueCollection nvc)
    {
        if (nvc is null)
            return string.Empty;

        var builder = new StringBuilder();

        foreach (string key in nvc.Keys)
        {
            var values = nvc.GetValues(key);
            if (values is null)
                continue;

            foreach (var value in values)
            {
                builder.Append(builder.Length == 0 ? "" : "&");
                builder.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }
        }

        return builder.ToString();
    }

    private static void AddPairsToToken(JToken token, ref NameValueCollection pairs, string prefix = "")
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                AddPairsToToken(property.Value, ref pairs, key);
            }
        }
        else if (token is JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                AddPairsToToken(array[i], ref pairs, $"{prefix}[{i}]");
            }
        }
        else if (token.Type != JTokenType.Null)
        {
            pairs.Add(prefix, token.ToString());
        }
    }
}
