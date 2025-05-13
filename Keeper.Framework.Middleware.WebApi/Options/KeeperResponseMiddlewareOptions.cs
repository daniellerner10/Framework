using Keeper.Framework.Validations;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Keeper.Framework.Middleware;

/// <summary>
/// Options for configuring keeper response middleware.
/// </summary>
public class KeeperWebApiResponseMiddlewareOptions
{
    /// <summary>
    /// Enable the unified resopnse.  Defaults to true.
    /// </summary>
    public bool EnableUnifiedResponse { get; set; } = true;

    /// <summary>
    /// Whether to camel case the unified resopnse.  Defaults to false.
    /// </summary>
    public bool CamelCaseUnifiedResponse { get; set; } = false;

    /// <summary>
    /// Enable exception handling.  Defaults to true.
    /// </summary>
    public bool EnableExceptionHandling { get; set; } = true;

    /// <summary>
    /// Enables log context.  Defaults to true.
    /// </summary>
    public bool EnableLogContext { get; set; } = true;

    /// <summary>
    /// Enables application state.  Defaults to true.
    /// </summary>
    public bool EnableApplicationState { get; set; } = true;

    /// <summary>
    /// Configures logging policy.  Defaults to <see cref="LoggingPolicy.UnhandledExceptionsOnly"/>.
    /// </summary>
    public LoggingPolicy LoggingPolicy { get; set; } = LoggingPolicy.UnhandledExceptionsOnly;

    /// <summary>
    /// Fires before every request.
    /// </summary>
    public Func<IServiceProvider, HttpContext, Guid, Task>? OnBeforeRequest { get; set; } = null;

    /// <summary>
    /// Fires after every request.
    /// </summary>
    public Func<IServiceProvider, HttpContext, Guid, HttpStatusCode, Task>? OnAfterRequest { get; set; } = null;

    /// <summary>
    /// Fires on an exception.
    /// </summary>
    public Func<IServiceProvider, HttpContext, Guid, Exception, ErrorResponse?, HttpStatusCode?, Task>? OnRequestError { get; set; } = null;

    /// <summary>
    /// Api filters.  Any uri paths that contain one of the items on this list will not use the middleware.
    /// </summary>

    public List<string> ApiLogRequestFilters { get; set; } = [];

    public List<string> ApiLogResponseFilters { get; set; } = [];

    public List<string> ApiLogRequestResponseFilters { get; set; } = [];

    public List<string> ApiFilters { get; set; } = ["/swagger"];
}