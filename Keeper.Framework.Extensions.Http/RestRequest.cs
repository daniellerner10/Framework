using Keeper.Framework.Serialization.Json;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;

namespace Keeper.Framework.Extensions.Http;

/// <summary>
/// A rest request with a strongly typed body.
/// </summary>
/// <typeparam name="TBody">The type of the body.</typeparam>
public class RestRequest<TBody> : RestRequest, IRestRequest<TBody>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    public RestRequest(TBody body) : base(HttpMethod.Post)
    {
        InitBody(body);
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="uri">The uri.</param>
    public RestRequest(TBody body, Uri uri) : this(body, uri.ToString())
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="resource">The resource.</param>
    public RestRequest(TBody body, string resource) : base(HttpMethod.Post, resource)
    {
        InitBody(body);
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="jsonSettings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the body.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    public RestRequest(TBody body, Uri uri, JsonSerializerSettings jsonSettings) : this(body, uri.ToString(), jsonSettings)
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="jsonSettings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the body.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    public RestRequest(TBody body, string resource, JsonSerializerSettings jsonSettings) : base(HttpMethod.Post, resource)
    {
        InitBody(body, jsonSettings);
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="method">The method.</param>
    public RestRequest(TBody body, HttpMethod method) : base(method)
    {
        InitBody(body);
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="method">The method.</param>
    /// <param name="uri">The uri.</param>
    public RestRequest(TBody body, HttpMethod method, Uri uri) : this(body, method, uri.ToString())
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="method">The method.</param>
    /// <param name="resource">The resource.</param>
    public RestRequest(TBody body, HttpMethod method, string resource) : base(method, resource)
    {
        InitBody(body);
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="method">The method.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="jsonSettings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the body.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    public RestRequest(TBody body, HttpMethod method, Uri uri, JsonSerializerSettings jsonSettings) : this(body, method, uri.ToString(), jsonSettings)
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="method">The method.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="jsonSettings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the body.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    public RestRequest(TBody body, HttpMethod method, string resource, JsonSerializerSettings jsonSettings) : base(method, resource)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        InitBody(body, jsonSettings);
    }

    /// <summary>
    /// The typed body of the rest reqeuest.
    /// </summary>
    public TBody Body { get; set; }

    private void InitBody(TBody body, JsonSerializerSettings? jsonSettings = null)
    {
        if (body is null)
            throw new ArgumentNullException(nameof(body));

        Body = body;
        if (HttpMethod.AllowPayloadInBody())
            StringContent = JsonConvert.SerializeObject(body, Formatting.Indented, jsonSettings);
        else
            this.ApplyObjectAsQueryString(body);
    }
}

/// <summary>
/// RestRequest used for Http requests
/// </summary>
public class RestRequest : IRestRequest
{
    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    public RestRequest() : this(HttpMethod.Get)
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    public RestRequest(HttpMethod method) : this(method, string.Empty)
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="resource">The resource.</param>
    public RestRequest(string resource) : this(HttpMethod.Get, resource)
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="uri">The uri.</param>
    public RestRequest(HttpMethod method, Uri uri) : this(method, uri.ToString())
    {
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="resource">The resource.</param>
    public RestRequest(HttpMethod method, string resource)
    {
        HttpMethod = method;
        Resource = resource;
        Headers = [];
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="encoding">The encoding.</param>
    public RestRequest(HttpMethod method, Uri uri, Encoding? encoding = null) : this(method, uri.ToString(), encoding)
    {

    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="encoding">The encoding.</param>
    public RestRequest(HttpMethod method, string resource, Encoding? encoding = null)
        : this(method, resource)
    {
        if (encoding != null)
            Encoding = encoding;
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="content">The content.</param>
    /// <param name="encoding">The encoding.</param>
    public RestRequest(HttpMethod method, Uri uri, string content, Encoding? encoding = null)
        : this(method, uri.ToString(), content, encoding)
    {

    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="content">The content.</param>
    /// <param name="encoding">The encoding.</param>
    public RestRequest(HttpMethod method, string resource, string content, Encoding? encoding = null)
        : this(method, resource, encoding)
    {
        StringContent = content;
    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="content">The content.</param>
    /// <param name="encoding">The encoding.</param>
    public RestRequest(HttpMethod method, Uri uri, byte[] content, Encoding? encoding = null)
        : this(method, uri.ToString(), content, encoding)
    {

    }

    /// <summary>
    /// Constructor for rest request.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="content">The content.</param>
    /// <param name="encoding">The encoding.</param>
    public RestRequest(HttpMethod method, string resource, byte[] content, Encoding? encoding = null)
        : this(method, resource, encoding)
    {
        Content = content;
    }

    /// <summary>
    /// The http method.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(NewtonsoftHttpMethodConverter))]
    public HttpMethod HttpMethod { get; set; }

    /// <summary>
    /// The resource of the request.
    /// </summary>
    public string Resource { get; set; } = default!;

    /// <summary>
    /// The request uri.
    /// </summary>
    public Uri RequestUri { get; internal set; } = default!;

    /// <summary>
    /// The encoding of the body.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// The body as bytes.
    /// </summary>
    public byte[]? Content { get; set; }

    private string? _stringContent;

    /// <summary>
    /// The body as a string.
    /// </summary>
    public string? StringContent
    {
        get => _stringContent ??= Content == null ? null : Encoding.GetString(Content);
        set
        {
            _stringContent = value;
            Content = _stringContent is null ? null : Encoding.GetBytes(_stringContent);
        }
    }

    /// <summary>
    /// The headers of the request.
    /// </summary>
    public NameValueCollection Headers { get; } = default!;

    /// <summary>
    /// Does the request have headers.
    /// </summary>
    public bool HasHeaders => Headers.HasKeys();

    private AuthenticationHeaderValue _authenticationHeaderValue = default!;
    /// <summary>
    /// Add authentication header value.
    /// </summary>
    AuthenticationHeaderValue IRestRequest.AuthenticationHeaderValue => _authenticationHeaderValue;

    /// <summary>
    /// Add authorization header
    /// </summary>
    /// <param name="authenticationHeaderValue">The header value.</param>
    public void AddAuthorizationHeader(AuthenticationHeaderValue authenticationHeaderValue)
    {
        _authenticationHeaderValue = authenticationHeaderValue;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $@"{HttpMethod} {RequestUri?.ToString() ?? Resource}

{SerializeHeaders()}
{StringContent}";
    }

    private string SerializeHeaders()
    {
        if (Headers is null)
            return string.Empty;

        var builder = new StringBuilder();

        foreach (string key in Headers.Keys)
            builder.AppendLine($"{key}: {Headers[key]}");

        if (_authenticationHeaderValue is not null)
            builder.AppendLine($"authorization: {MaskAuthorizationHeader()}");

        return builder.ToString();
    }

    private string MaskAuthorizationHeader()
    {
        var tokenValue = _authenticationHeaderValue.Parameter;
        if (tokenValue is null)
            return _authenticationHeaderValue.Scheme;

        if (tokenValue.Length <= 6)
            tokenValue = new string('*', tokenValue.Length);
        else
            tokenValue = $"{tokenValue[..4]}{new string('*', tokenValue.Length - 4)}";

        return $"{_authenticationHeaderValue.Scheme} {tokenValue}";
    }
}