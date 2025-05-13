using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Keeper.Framework.Extensions.Http;

/// <summary>
/// Container for data sent back from API including deserialized data and the request
/// that was made to get this response
/// </summary>
public class RestResponse : IRestResponse
{
    private string? _content;

    /// <summary>
    /// Default constructor
    /// </summary>
    public RestResponse()
    {
    }

    /// <summary>
    /// The response headers.
    /// </summary>
    public NameValueCollection? Headers { get; internal set; }

    /// <summary>
    /// MIME content type of response
    /// </summary>
    public string? ContentType { get; internal set; }

    /// <summary>
    /// Length in bytes of the response content
    /// </summary>
    public long ContentLength { get; internal set; }

    /// <summary>
    /// String representation of response content
    /// </summary>
    public string Content
    {
        get => _content ??= RawBytes == null ? "" : Encoding.UTF8.GetString(RawBytes, 0, RawBytes.Length);
        internal set => _content = value;
    }

    /// <summary>
    /// Response content
    /// </summary>
    public byte[]? RawBytes { get; internal set; }

    /// <summary>
    /// HTTP response status code
    /// </summary>
    public HttpStatusCode? StatusCode { get; internal set; }

    /// <summary>
    /// Whether or not the response status code indicates success
    /// </summary>
    public bool IsSuccessful { get; internal set; }

    /// <summary>
    /// Whether or not the response error is transient and expected to succeed subsequently.
    /// This can be used to decide if an immediate retry should be implemented.
    /// </summary>
    public bool IsTransientError => !IsSuccessful && ((int)StatusCode! switch
    {
        408 => true,
        >= 500 => true,
        _ => false
    } || ErrorException is HttpRequestException);

    /// <summary>
    /// The error message.
    /// </summary>
    public string? ErrorMessage { get; internal set; }

    /// <summary>
    /// The error exception.
    /// </summary>
    public Exception? ErrorException { get; internal set; }

    /// <summary>
    /// The Request that was made to get this RestResponse
    /// </summary>
    public IRestRequest? Request { get; internal set; }

    /// <summary>
    /// Description of HTTP status returned
    /// </summary>
    public string? StatusDescription { get; internal set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append($@"Status: {StatusCode}

{SerializeHeaders()}
{Content}");

        if (ErrorMessage is not null)
            builder.Append($"{Environment.NewLine}{Environment.NewLine}ErrorMessage: {ErrorMessage}");

        if (ErrorException is not null)
            builder.Append($"{Environment.NewLine}{Environment.NewLine}ErrorException: {ErrorException}");

        return builder.ToString();
    }

    private string SerializeHeaders()
    {
        if (Headers is null)
            return string.Empty;

        var builder = new StringBuilder();

        foreach (string key in Headers.Keys)
            builder.AppendLine($"{key}: {Headers[key]}");

        return builder.ToString();
    }
}

/// <summary>
/// Container for data sent back from API including deserialized data
/// </summary>
/// <typeparam name="TResponseData">Type of data to deserialize to</typeparam>
public class RestResponse<TResponseData> : RestResponse, IRestResponse<TResponseData>, IRestResponseWithSerializer
{
    /// <summary>
    /// Deserialized entity data
    /// </summary>
    public TResponseData Data { get; internal set; } = default!;

    void IRestResponseWithSerializer.DeserializeBody()
    {
        Data = JsonConvert.DeserializeObject<TResponseData>(Content)!;
    }
}

/// <summary>
/// The rest resonse.
/// </summary>
/// <typeparam name="TResponseData">The response data.</typeparam>
/// <typeparam name="TRequestData">The request data.</typeparam>
public class RestResponse<TResponseData, TRequestData> : RestResponse<TResponseData>, IRestResponse<TResponseData, TRequestData>
{
    IRestRequest<TRequestData>? IRestResponse<TResponseData, TRequestData>.Request => Request as IRestRequest<TRequestData>;
}

internal interface IRestResponseWithSerializer
{
    void DeserializeBody();
}
