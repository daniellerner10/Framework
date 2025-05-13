using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;

namespace Keeper.Framework.Extensions.Http;

/// <summary>
/// Represents an api request.
/// </summary>
/// <typeparam name="TBody">The type of the request data.</typeparam>
public interface IRestRequest<TBody> : IRestRequest
{
    /// <summary>
    /// The body.
    /// </summary>
    TBody Body { get; set; }
}

/// <summary>
/// Represents an api request.
/// </summary>
public interface IRestRequest
{
    /// <summary>
    /// Determines what HTTP method to use for this request. Supported methods: GET, POST, PUT, DELETE, HEAD, OPTIONS
    /// Default is GET
    /// </summary>
    HttpMethod HttpMethod { get; set; }

    /// <summary>
    /// The Resource URL to make the request against.
    /// Tokens are substituted with UrlSegment parameters and match by name.
    /// Should not include the scheme or domain. Do not include leading slash.
    /// Combined with RestClient.BaseUrl to assemble final URL:
    /// {BaseUrl}/{Resource} (BaseUrl is scheme + domain, e.g. http://example.com)
    /// </summary>
    /// <example>
    /// // example for url token replacement
    /// request.Resource = "Products/{ProductId}";
    /// request.AddParameter("ProductId", 123, ParameterType.UrlSegment);
    /// </example>
    string Resource { get; set; }

    /// <summary>
    /// The request uri.
    /// </summary>
    Uri RequestUri { get; }

    /// <summary>
    /// The request content as bytes.
    /// </summary>
    byte[]? Content { get; set; }

    /// <summary>
    /// The request content as string.
    /// </summary>
    string? StringContent { get; set; }

    /// <summary>
    /// The encoding of the body.
    /// </summary>
    Encoding Encoding { get; set; }

    /// <summary>
    /// The headers of the request.
    /// </summary>
    NameValueCollection Headers { get; }

    /// <summary>
    /// True if request has headers.
    /// </summary>
    bool HasHeaders { get; }

    /// <summary>
    /// Add authorization header.
    /// </summary>
    /// <param name="authenticationHeaderValue">The authorization header value.</param>
    void AddAuthorizationHeader(AuthenticationHeaderValue authenticationHeaderValue);

    internal AuthenticationHeaderValue AuthenticationHeaderValue { get; }
}