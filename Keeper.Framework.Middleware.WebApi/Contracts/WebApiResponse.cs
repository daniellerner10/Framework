using System.Net;

namespace Keeper.Framework.Middleware;

/// <summary>
/// Unified web api response.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class WebApiResponse<TResult>
{
    /// <summary>
    /// The version of the api.
    /// </summary>
    public string Version { get; set; } = default!;
    /// <summary>
    /// The http status code of the result.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
    /// <summary>
    /// Was the request successful or not?
    /// </summary>
    public bool IsSuccessful { get; set; }
    /// <summary>
    /// The result if request was successful.
    /// </summary>
    public TResult? Result { get; set; }
    /// <summary>
    /// The errors if the request was not successful.
    /// </summary>
    public IList<ErrorDetail>? Errors { get; set; }
}