using System.Collections.Specialized;
using System.Net;

namespace Keeper.Framework.Extensions.Http;

/// <summary>
/// Represents a rest response.
/// </summary>
/// <typeparam name="TData">The type of the expected response data.</typeparam>
/// <typeparam name="TRequestBody">The type of the request body.</typeparam>
public interface IRestResponse<out TData, TRequestBody> : IRestResponse<TData>
{
    /// <summary>
    /// The request object.
    /// </summary>
    new IRestRequest<TRequestBody>? Request { get; }
}

/// <summary>
/// Container for data sent back from API including deserialized data and the request
/// that was made to get this response
/// </summary>
/// <typeparam name="TData">Type of data to deserialize to</typeparam>
public interface IRestResponse<out TData> : IRestResponse
{
    /// <summary>
    /// Deserialized entity data
    /// </summary>
    TData Data { get; }
}

/// <summary>
/// Container for data sent back from API
/// </summary>
public interface IRestResponse
{
    /// <summary>
    /// Headers returned by server with the response
    /// </summary>
    NameValueCollection? Headers { get; }

    /// <summary>
    /// MIME content type of response
    /// </summary>
    string? ContentType { get; }

    /// <summary>
    /// Length in bytes of the response content
    /// </summary>
    public long ContentLength { get; }

    /// <summary>
    /// String representation of response content
    /// </summary>
    string? Content { get; }

    /// <summary>
    /// HTTP response status code
    /// </summary>
    HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Description of HTTP status returned
    /// </summary>
    string? StatusDescription { get; }

    /// <summary>
    /// Whether or not the response status code indicates success
    /// </summary>
    bool IsSuccessful { get; }

    /// <summary>
    /// Whether or not the error is transient and can be expected to succeed subsequently.
    /// This can be used to decide if an immediate retry should be implemented.
    /// </summary>
    bool IsTransientError { get; }

    /// <summary>
    /// Transport or other non-HTTP error generated while attempting request
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Exceptions thrown during the request, if any.
    /// </summary>
    /// <remarks>
    /// Will contain only network transport or framework exceptions thrown during the request.
    /// </remarks>
    Exception? ErrorException { get; }

    /// <summary>
    /// The Request that was made to get this RestResponse
    /// </summary>
    IRestRequest? Request { get; }
}