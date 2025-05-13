using Keeper.Framework.Application.State;
using Keeper.Framework.Collections;
using Keeper.Framework.Extensions;
using Keeper.Framework.Logging;
using Keeper.Framework.Middleware.WebApi.Contracts;
using Keeper.Framework.Validations;
using Keeper.Masking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Keeper.Framework.Middleware;

/// <summary>
/// Keeper response middleware.
/// </summary>
public class KeeperWebApiResponseMiddleware
{
    private readonly static Type _httpResponseStreamAttributeType = typeof(HttpResponseStreamAttribute);
    private readonly static Type _httpRequestStreamAttributeType = typeof(HttpRequestStreamAttribute);

    internal static LoggingPolicy LoggingPolicy { get; set; }
    internal static bool EnableExceptionHandling { get; set; }
    internal static bool EnableUnifiedResponse { get; set; }
    internal static bool CamelCaseUnifiedResponse { get; set; }

    internal static bool EnableLogContext { get; set; }
    internal static bool EnableApplicationState { get; set; } = true;

    internal static Func<IServiceProvider, HttpContext, Guid, Task>? OnBeforeRequest { get; set; }
    internal static Func<IServiceProvider, HttpContext, Guid, HttpStatusCode, Task>? OnAfterRequest { get; set; }
    internal static Func<IServiceProvider, HttpContext, Guid, Exception, ErrorResponse?, HttpStatusCode?, Task>? OnRequestError { get; set; }
    internal static List<string> ApiFilters { get; set; } = [];

    private const string CONTENT_TYPE = "application/json";

    private static string? _propNameVersion;
    private static string PropNameVersion => _propNameVersion ??= CamelCaseUnifiedResponse ? "version" : "Version";

    private static string? _propNameIsSuccessful;
    private static string PropNameIsSuccessful => _propNameIsSuccessful ??= CamelCaseUnifiedResponse ? "isSuccessful" : "IsSuccessful";

    private static string? _propNameStatusCode;
    private static string PropNameStatusCode => _propNameStatusCode ??= CamelCaseUnifiedResponse ? "statusCode" : "StatusCode";

    private static string? _propNameResult;
    private static string PropNameResult => _propNameResult ??= CamelCaseUnifiedResponse ? "result" : "Result";

    private static string? _propNameErrors;
    private static string PropNameErrors => _propNameErrors ??= CamelCaseUnifiedResponse ? "errors" : "Errors";

    private static JsonSerializerOptions? _jsonSerializerOptions;
    private static JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions ??= CreateJsonSerializerOptions();

    private ApiLogFilters _apiLogFilters;

    public List<string> ApiLogRequestFilters => _apiLogFilters.ApiLogRequestFilters;

    public List<string> ApiLogResponseFilters => _apiLogFilters.ApiLogResponseFilters;

    public List<string> ApiLogRequestResponseFilters => _apiLogFilters.ApiLogRequestResponseFilters;

    /// <summary>
    /// The default version.
    /// </summary>
    public const string DefaultVersion = "1.0";

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = CamelCaseUnifiedResponse ? JsonNamingPolicy.CamelCase : null
        };
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    private static JsonWriterOptions JsonWriterOptions = new()
    {
        Indented = true,
        SkipValidation = true
    };

    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KeeperWebApiResponseMiddleware> _logger;

    /// <summary>
    /// Consturctor for Keeper response middleware.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="next">The request delegate for calling next layer in request.</param>
    public KeeperWebApiResponseMiddleware(IServiceProvider serviceProvider, ApiLogFilters apiLogFilters, RequestDelegate next)
    {
        _serviceProvider = serviceProvider;
        _apiLogFilters = apiLogFilters;
        _next = next;
        _logger = serviceProvider.GetService<ILogger<KeeperWebApiResponseMiddleware>>() ?? NullLogger<KeeperWebApiResponseMiddleware>.Instance;
    }

    /// <summary>
    /// Gets invoked on every request.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <returns>Task to wait upon.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        var keeperRequestContext = context.GetKeeperRequestContext();

        if (EnableApplicationState)
        {
            using (KeeperApplicationContext.PushState(keeperRequestContext))
            {
                return InvokeAsyncInternal(context, keeperRequestContext);
            }
        }
        else
            return InvokeAsyncInternal(context, keeperRequestContext);
    }

    private async Task InvokeAsyncInternal(HttpContext context, IKeeperRequestContext keeperRequestContext)
    {
        if (!ShouldFilter(context))
        {
            if (LoggingPolicy == LoggingPolicy.All)
            {
                await LogRequest(context, keeperRequestContext);
            }

            if (IsStreamingEndpoint(context, _httpResponseStreamAttributeType))
            {
                await HandleHttpStream(context, keeperRequestContext);
            }
            else
            {
                var originalBody = context.Response.Body;
                var body = new MemoryStream();
                context.Response.Body = body;

                if (OnBeforeRequest != null)
                    await OnBeforeRequest(_serviceProvider, context, keeperRequestContext.CorrelationId);

                try
                {
                    if (EnableUnifiedResponse)
                    {
                        await _next(context);

                        var bodyString = await GetResponseBodyAsString(context.Response);

                        await HandleSuccess(context, bodyString, context.Response.StatusCode);
                    }
                    else
                    {
                        await _next(context);
                    }
                }
                catch (Exception ex)
                {
                    if (LoggingPolicy != LoggingPolicy.None)
                    {
                        _logger.LogError(ex, "Failed to process request, CorrelationId: {CorrelationId}", keeperRequestContext.CorrelationId);
                    }
                    if (EnableExceptionHandling)
                    {
                        var (error, code) = MapExceptionToResponse(ex);

                        if (OnRequestError != null)
                            await OnRequestError(_serviceProvider, context, keeperRequestContext.CorrelationId, ex, error, code);

                        await HandleExceptionAsync(context, error, code);
                    }
                    else
                    {
                        if (OnRequestError != null)
                            await OnRequestError(_serviceProvider, context, keeperRequestContext.CorrelationId, ex, null, HttpStatusCode.InternalServerError);

                        throw;
                    }
                }
                finally
                {
                    if (OnAfterRequest != null)
                        await OnAfterRequest(_serviceProvider, context, keeperRequestContext.CorrelationId, (HttpStatusCode)context.Response.StatusCode);

                    if (LoggingPolicy == LoggingPolicy.All)
                    {
                        var finalBody = await GetResponseBodyAsString(context.Response);
                        Log(LogType.Response, context, finalBody, keeperRequestContext);
                    }

                    await FinalizeResponse(context, originalBody, body);
                }
            }
        }
        else
            await _next(context);
    }

    private async Task HandleHttpStream(HttpContext context, IKeeperRequestContext keeperRequestContext)
    {
        if (OnBeforeRequest != null)
            await OnBeforeRequest(_serviceProvider, context, keeperRequestContext.CorrelationId);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (LoggingPolicy != LoggingPolicy.None)
                _logger.LogError(ex, "Failed to process request, CorrelationId: {CorrelationId}", keeperRequestContext.CorrelationId);
            if (EnableExceptionHandling)
            {
                var (error, code) = MapExceptionToResponse(ex);

                if (OnRequestError != null)
                    await OnRequestError(_serviceProvider, context, keeperRequestContext.CorrelationId, ex, error, code);

                await WriteErrorToResponseBody(context, error, code);
            }
            else
            {
                if (OnRequestError != null)
                    await OnRequestError(_serviceProvider, context, keeperRequestContext.CorrelationId, ex, null, HttpStatusCode.InternalServerError);

                throw;
            }
        }
        finally
        {
            if (OnAfterRequest != null)
                await OnAfterRequest(_serviceProvider, context, keeperRequestContext.CorrelationId, (HttpStatusCode)context.Response.StatusCode);

            if (LoggingPolicy == LoggingPolicy.All)
            {
                Log(LogType.Response, context, "Http streaming does not support the logging of the response stream", keeperRequestContext);
            }
        }
    }

    private static bool IsStreamingEndpoint(HttpContext context, Type type) =>
        context.GetEndpoint()
               ?.Metadata
               .GetMetadata<ControllerActionDescriptor>()
               ?.MethodInfo
               .CustomAttributes
               ?.Any(x => x.AttributeType == type) ?? false;

    private static Task FinalizeResponse(HttpContext context, Stream originalBody, MemoryStream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        context.Response.ContentLength = body.Length;
        return body.CopyToAsync(originalBody);
    }

    private async Task LogRequest(HttpContext context, IKeeperRequestContext keeperRequestContext)
    {
        var request = context.Request;

        string? bodyAsText = null;
        if (!request.Method.Equals(HttpMethod.Get.Method, StringComparison.InvariantCultureIgnoreCase) && request.Body != null)
        {
            var isRequestStream = IsStreamingEndpoint(context, _httpRequestStreamAttributeType);
            if (isRequestStream)
                bodyAsText = "Http streaming does not support the logging of the response stream";
            else
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, leaveOpen: true);

                bodyAsText = await reader.ReadToEndAsync();

                request.Body.Seek(0, SeekOrigin.Begin);
                request.EnableBuffering();
            }
        }

        Log(LogType.Request, context, bodyAsText, keeperRequestContext);
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "We want to use this method.")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "It is necessary.")]
    private void Log(LogType logType, HttpContext context, string? body, IKeeperRequestContext keeperRequestContext)
    {
        if ((logType == LogType.Request  && ShouldFilterLogRequest(context) ) ||
            (logType == LogType.Response && ShouldFilterLogResponse(context)))
                return; 

        var request = context.Request;

        var builder = new StringBuilder();
        var args = new List<object?>();

        var username = context.User.FindFirst(ClaimTypes.NameIdentifier);

        builder.Append($"Api {logType}: ");
        AddLogParameter(builder, args, "ApiLogEntryId", keeperRequestContext.CorrelationId);
        AddLogParameter(builder, args, "User", username!);
        AddLogParameter(builder, args, "ClientIpAddress", context.Connection.RemoteIpAddress?.ToString());
        AddLogParameter(builder, args, "Machine", Environment.MachineName);
        AddLogParameter(builder, args, "RequestIpAddress", context.Connection.LocalIpAddress?.ToString());
        AddLogParameter(builder, args, "RequestMethod", request.Method);
        AddLogParameter(builder, args, "RequestContentType", request.ContentType);

        SerializeAndLogHeaders(builder, args, logType, context);

        AddLogParameter(builder, args, $"{logType}Timestamp", DateTime.UtcNow);
        AddLogParameter(builder, args, "RequestUri", request.GetDisplayUrl());
        AddLogParameter(builder, args, $"{logType}ContentBody", MaskBodyIfNecessary(logType, context, body));

        using (LoggerContext.LoggerEnterSensitiveArea())
        {
            _logger.LogInformation(builder.ToString(), [.. args]);
        }
    }

    private static string? MaskBodyIfNecessary(LogType logType, HttpContext context, string? body)
    {
        if (body is null)
            return null;

        var endpoint = context.GetEndpoint();
        if (endpoint is not null)
        {
            var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor is not null)
            {
                return logType switch
                {
                    LogType.Request => MaskRequestBodyIfNecessary(controllerActionDescriptor, body),
                    LogType.Response => MaskResponseBodyIfNecessary(controllerActionDescriptor, body),
                    _ => throw new NotSupportedException($"Log type '{logType}' is not supported.")
                };
            }
            else
                return body;
        }
        else
            return body;
    }

    private static readonly SynchronizedDictionary<ControllerActionDescriptor, Type?> _requestTypeCache = [];
    private static string MaskRequestBodyIfNecessary(ControllerActionDescriptor descriptor, string body)
    {
        var bodyParameter = _requestTypeCache.GetOrAdd(
            descriptor, 
            d => d.Parameters.FirstOrDefault(x => x.BindingInfo?.BindingSource?.Id == "Body")?.ParameterType
        );

        if (bodyParameter is null)
            return body;
        else
            return bodyParameter.ApplyMaskToJson(body);
    }

    private static readonly SynchronizedDictionary<ControllerActionDescriptor, Type> _responseTypeCache = [];
    private static string MaskResponseBodyIfNecessary(ControllerActionDescriptor descriptor, string body)
    {
        var bodyParameter = _responseTypeCache.GetOrAdd(
            descriptor,
            d => GetResponseTypeFromMethodInfo(descriptor.MethodInfo)
        );

        var responseType = GetResponseTypeFromMethodInfo(descriptor.MethodInfo);

        return responseType.ApplyMaskToJson(body);
    }

    private static Type GetResponseTypeFromMethodInfo(MethodInfo methodInfo)
    {
        var responseType = methodInfo.ReturnType;

        if (responseType.Name == "Task`1")
            responseType = responseType.GenericTypeArguments[0];

        if (responseType.Name == "ActionResult`1")
            responseType = responseType.GenericTypeArguments[0];

        return responseType;
    }

    private static readonly string _authorizationHeaderTokenGroupName = "token";
    private static readonly Regex _authorizationHeaderRegex = new($"(?:basic|bearer)\\s+(?<{_authorizationHeaderTokenGroupName}>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static string SerializeHeadersWithAuthorizationMasking(IHeaderDictionary headers)
        => JsonSerializer.Serialize(headers.Select(x =>
        {
            if (x.Key.Equals(HeaderNames.Authorization, StringComparison.InvariantCultureIgnoreCase))
            {
                var notMasked = _authorizationHeaderRegex.Match(x.Value!).Groups[_authorizationHeaderTokenGroupName].Value;

                string masked;
                if (notMasked.Length > 4)
                    masked = $"{new string('*', notMasked.Length - 4)}{notMasked.SafeSubstring(notMasked.Length - 4)}";
                else
                    masked = "****";

                return KeyValuePair.Create(x.Key, new StringValues(masked));
            }
            return x;
        }));

    private static void SerializeAndLogHeaders(StringBuilder builder, List<object?> args, LogType logType, HttpContext context)
    {
        string? json;
        string? key;
        IHeaderDictionary? headers;

        if (logType == LogType.Response)
        {
            headers = context.Response.Headers;
            key = "ResponseHeaders";
            json = JsonSerializer.Serialize(headers);
        }
        else //if (logType == LogType.Request)
        {
            headers = context.Request.Headers;
            key = "RequestHeaders";
            json = SerializeHeadersWithAuthorizationMasking(headers);
        }

        AddLogParameter(builder, args, key, json);
    }

    private static void AddLogParameter(StringBuilder builder, List<object?> args, string key, object? value)
    {
        if (builder.Length > 0)
            builder.Append(", ");

        builder.Append($"{key}: {{{key}}}");
        args.Add(value);
    }

    private static bool ShouldFilter(HttpContext context) =>
        context.Request.Path.Value is not null &&
        ApiFilters.Any(f => context.Request.Path.Value.Contains(f, StringComparison.InvariantCultureIgnoreCase));

    private bool ShouldFilterLogRequest(HttpContext context) =>
        context.Request.Path.Value is not null &&
        (ApiLogRequestFilters.Any(f => context.Request.Path.Value.Contains(f, StringComparison.InvariantCultureIgnoreCase)) ||
         ApiLogRequestResponseFilters.Any(f => context.Request.Path.Value.Contains(f, StringComparison.InvariantCultureIgnoreCase)));

    private bool ShouldFilterLogResponse(HttpContext context) =>
        context.Request.Path.Value is not null &&
        (ApiLogResponseFilters.Any(f => context.Request.Path.Value.Contains(f, StringComparison.InvariantCultureIgnoreCase)) ||
         ApiLogRequestResponseFilters.Any(f => context.Request.Path.Value.Contains(f, StringComparison.InvariantCultureIgnoreCase)));

    private static Task HandleSuccess(HttpContext context, string result, int statusCode)
    {
        var response = new WebApiResponse<string>
        {
            IsSuccessful = statusCode < 300 && statusCode >= 200,
            StatusCode = (HttpStatusCode)statusCode,
            Version = GetApiVersion(context)
        };

        if (response.IsSuccessful)
            response.Result = result;
        else
        {
            try
            {
                var genericResponse = JsonSerializer.Deserialize<GenericMiddlewareResponse>(result, JsonSerializerOptions);
                if (genericResponse?.Errors.Count > 0)
                {
                    var code = GetErrorCodeFromStatusCode(statusCode);

                    response.Errors = genericResponse.Errors.Select(pair =>
                        new ErrorDetail
                        {
                            Code = code,
                            Message = string.Format("Validation error(s) on field {0}: [\"{1}\"]", pair.Key, string.Join("\",\"", pair.Value.Select(v => (v))))
                        }).ToList();
                }
                else
                {
                    response.Errors = GetError(ref result, statusCode, genericResponse);
                }
            }
            catch (Exception)
            {
                response.Errors = GetError(ref result, statusCode, null);
            }
        }


        return WriteResponse(context, response);
    }

    private static List<ErrorDetail> GetError(ref string result, int statusCode, GenericMiddlewareResponse? genericMiddlewareResponse)
    {
        return [
            new() {
                Code = GetErrorCodeFromStatusCode(statusCode),
                Message = GetErrorMessageFromStatusCode(statusCode, ref result, genericMiddlewareResponse)
            }
        ];
    }

    private static int GetErrorCodeFromStatusCode(int statusCode) =>
        statusCode switch
        {
            301 => ErrorCodes.System,     /* Moved */
            302 => ErrorCodes.System,     /* Redirect */
            303 => ErrorCodes.System,     /* SeeOther */
            304 => ErrorCodes.System,     /* NotModified */
            305 => ErrorCodes.System,     /* UseProxy */
            306 => ErrorCodes.System,     /* Unused */
            307 => ErrorCodes.System,     /* TemporaryRedirect */
            308 => ErrorCodes.System,     /* PermanentRedirect */
            400 => ErrorCodes.Validation, /* BadRequest */
            401 => ErrorCodes.Security,   /* Unauthorized */
            402 => ErrorCodes.Application,/* PaymentRequired */
            403 => ErrorCodes.Security,   /* Forbidden */
            404 => ErrorCodes.Application,/* NotFound */
            405 => ErrorCodes.Security,   /* MethodNotAllowed */
            406 => ErrorCodes.Application,/* NotAcceptable */
            407 => ErrorCodes.Security,   /* ProxyAuthenticationRequired */
            408 => ErrorCodes.Application,/* RequestTimeout */
            409 => ErrorCodes.Security,   /* Conflict */
            410 => ErrorCodes.Application,/* Gone */
            411 => ErrorCodes.Validation, /* LengthRequired */
            412 => ErrorCodes.Validation, /* PreconditionFailed */
            413 => ErrorCodes.Validation, /* RequestEntityTooLarge */
            414 => ErrorCodes.Validation, /* RequestUriTooLong */
            415 => ErrorCodes.Validation, /* UnsupportedMediaType */
            416 => ErrorCodes.Validation, /* RequestedRangeNotSatisfiable */
            417 => ErrorCodes.Validation, /* ExpectationFailed */
            421 => ErrorCodes.Validation, /* MisdirectedRequest */
            422 => ErrorCodes.Validation, /* UnprocessableEntity */
            423 => ErrorCodes.Security,   /* Locked */
            424 => ErrorCodes.Application,/* FailedDependency */
            426 => ErrorCodes.Application,/* UpgradeRequired */
            428 => ErrorCodes.Validation, /* PreconditionRequired */
            429 => ErrorCodes.Application,/* TooManyRequests */
            431 => ErrorCodes.Validation, /* RequestHeaderFieldsTooLarge */
            451 => ErrorCodes.Validation, /* UnavailableForLegalReasons */
            500 => ErrorCodes.System,     /* InternalServerError */
            501 => ErrorCodes.Application,/* NotImplemented */
            502 => ErrorCodes.System,     /* BadGateway */
            503 => ErrorCodes.System,     /* ServiceUnavailable */
            504 => ErrorCodes.System,     /* GatewayTimeout */
            505 => ErrorCodes.System,     /* HttpVersionNotSupported */
            506 => ErrorCodes.System,     /* VariantAlsoNegotiates */
            507 => ErrorCodes.System,     /* InsufficientStorage */
            508 => ErrorCodes.System,     /* LoopDetected */
            510 => ErrorCodes.System,     /* NotExtended */
            511 => ErrorCodes.Security,   /* NetworkAuthenticationRequired */
            _ => ErrorCodes.System
        };

    private static string GetErrorMessageFromStatusCode(int statusCode, ref string result, GenericMiddlewareResponse? genericResponse) =>
        string.IsNullOrWhiteSpace(result) || IsGenericMiddlewareResponse(ref result, genericResponse) ? ReasonPhrases.GetReasonPhrase(statusCode) : result.Trim('"');

    private static bool IsGenericMiddlewareResponse(ref string result, GenericMiddlewareResponse? genericResponse)
    {
        if (genericResponse == null)
            return false;
        else if (!string.IsNullOrEmpty(genericResponse.Detail))
        {
            result = genericResponse.Detail;
            return false;
        }
        else if (!string.IsNullOrEmpty(genericResponse.Title))
        {
            result = genericResponse.Title;
            return false;
        }
        else
            return true;
    }

    private static async Task WriteResponse<TResult>(HttpContext context, WebApiResponse<TResult> response)
    {
        context.Response.ContentType = CONTENT_TYPE;
        context.Response.StatusCode = (int)response.StatusCode;

        if (context.Response.StatusCode == 204 || context.Response.StatusCode == 304 || context.Response.StatusCode < 200)
            return;

        var writer = new Utf8JsonWriter(context.Response.Body, JsonWriterOptions);

        writer.WriteStartObject();
        writer.WriteString(PropNameVersion, response.Version);
        writer.WriteBoolean(PropNameIsSuccessful, response.IsSuccessful);
        writer.WriteNumber(PropNameStatusCode, (int)response.StatusCode);

        if (response.Result is not string str)
            str = JsonSerializer.Serialize(response.Result, JsonSerializerOptions);
        else if (IsInvalidJson(str))
            str = JsonSerializer.Serialize(str);

        if (response.IsSuccessful)
        {
            writer.WritePropertyName(PropNameResult);
        }
        else
        {
            writer.WritePropertyName(PropNameErrors);

            str = JsonSerializer.Serialize(response.Errors, JsonSerializerOptions);
        }

        await writer.FlushAsync();
        if (str is null)
            writer.WriteNullValue();
        else
            context.Response.Body.Write(Encoding.UTF8.GetBytes(str).AsSpan());

        writer.WriteEndObject();
        await writer.FlushAsync();

        context.Response.Body.SetLength(context.Response.Body.Position);
    }

    internal static bool IsInvalidJson(string result)
    {
        var bytes = Encoding.UTF8.GetBytes(result);
        var reader = new Utf8JsonReader(bytes.AsSpan());
        try
        {
            return !JsonDocument.TryParseValue(ref reader, out _);
        }
        catch (Exception)
        {
            return true;
        }
    }

    internal static string GetApiVersion(HttpContext context)
    {
        var version = context.GetRequestedApiVersion()?.ToString();
        if (version is null)
            return DefaultVersion;
        var versionSplit = version.Split('.');
        if (versionSplit.Length < 2)
            return $"{version}.0";
        return string.Join('.', versionSplit, 0, 2);
    }

    private static async Task<string> GetResponseBodyAsString(HttpResponse response)
    {
        using var stream = new StreamReader(response.Body, leaveOpen: true);
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await stream.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return text;
    }

    internal static (ErrorResponse error, HttpStatusCode code) MapExceptionToResponse(Exception exception)
    {
        var (er, c) = exception switch
        {
            KeeperValidationException e => (e.Issues.ToValidationResponse(), HttpStatusCode.BadRequest),
            ValidationException e => (e.Message.ToValidationResponse(), HttpStatusCode.BadRequest),
            FluentValidation.ValidationException e => (e.ToKeeperValidationException().Issues.ToValidationResponse(), HttpStatusCode.BadRequest),
            ArgumentException e => (e.Message.ToValidationResponse(), HttpStatusCode.BadRequest),
            KeeperApplicationException e => (e.Message.ToApplicationResponse(), HttpStatusCode.BadRequest),
            NotFoundException e => (e.Message.ToApplicationResponse(), HttpStatusCode.NotFound),
            NoContentException e => (e.Message.ToApplicationResponse(), HttpStatusCode.NoContent),
            SecurityException e => (e.Message.ToSecurityResponse(), HttpStatusCode.Forbidden),
            System.Security.SecurityException e => (e.Message.ToSecurityResponse(), HttpStatusCode.Forbidden),
            UnauthorizedAccessException e => (e.Message.ToSecurityResponse(), HttpStatusCode.Forbidden),
            NotImplementedException e => (e.Message.ToSecurityResponse(), HttpStatusCode.Forbidden),
            ConflictException e => (e.Message.ToSecurityResponse(), HttpStatusCode.Conflict),
            _ => ("General Exception".ToSystemResponse(), HttpStatusCode.InternalServerError)
        };

        return (er, c);
    }

    private static Task HandleExceptionAsync(HttpContext context, ErrorResponse error, HttpStatusCode code)
    {
        if (EnableUnifiedResponse)
        {
            var response = new WebApiResponse<object>
            {
                IsSuccessful = false,
                StatusCode = code,
                Version = GetApiVersion(context),
                Result = null,
                Errors = error.Errors
            };

            return WriteResponse(context, response);
        }
        else
        {
            return WriteErrorToResponseBody(context, error, code, "application/json");
        }
    }

    private static Task WriteErrorToResponseBody(HttpContext context, ErrorResponse error, HttpStatusCode code, string? contentType = null)
    {
        var json = JsonSerializer.Serialize(error);

        if (contentType is not null)
            context.Response.ContentType = contentType;
        context.Response.StatusCode = (int)code;

        return code switch
        {
            HttpStatusCode.NoContent => Task.CompletedTask,
            _ => context.Response.WriteAsync(json)
        };
    }

    internal static void StepIntoForDebugger()
    {
        // Method intentionally left empty for debugging.
    }
}