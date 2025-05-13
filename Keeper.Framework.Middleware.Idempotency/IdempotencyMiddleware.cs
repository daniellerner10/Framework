using Keeper.Framework.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Keeper.Framework.Middleware.Idempotency;
internal class IdempotencyMiddleware
{
    internal static string DefaultConnectionString { get; set; } = default!;
    
    internal static string DefaultConflictResponseNotYetSavedResponse { get; set; } = "Request resent too early. Please try again soon.";

    internal static int DefaultConflictResponseNotYetSavedStatusCode { get; set; } = 425;

    internal static IdempotencyTables IdempotencyTables { get; set; } = default!;

    private readonly static Type IdempotentAttributeType = typeof(IdempotentAttribute);
    private readonly static IIdempotencyKeySelector DefaultIdempotencyKeySelector = new DefaultIdempotencyKeySelector();

    private readonly static IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            IdempotentAttribute? idempotentAttribute;
            var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor != null)
            {
                idempotentAttribute = Attribute.GetCustomAttribute(controllerActionDescriptor.MethodInfo, IdempotentAttributeType) as IdempotentAttribute;
                idempotentAttribute ??= Attribute.GetCustomAttribute(controllerActionDescriptor.ControllerTypeInfo, IdempotentAttributeType) as IdempotentAttribute;

                if (idempotentAttribute == null)
                    await _next(context);
                else
                    await ExecuteIdempotency(context, controllerActionDescriptor, idempotentAttribute);
            }
            else
                await _next(context);
        }
        else
            await _next(context);
    }

    private async Task ExecuteIdempotency(
        HttpContext context,
        ControllerActionDescriptor controllerActionDescriptor,
        IdempotentAttribute idempotentAttribute)
    {
        var tableName = idempotentAttribute.IdempotencyTable;
        if (!IdempotencyTables.TryGetValue(tableName, out var options))
            throw new InvalidOperationException($"Controller action wanted to use idempotency table {tableName} but it was not configured.");

        var keySelector = context.RequestServices.GetService<IIdempotencyKeySelector>() ?? DefaultIdempotencyKeySelector;
        var keyString = await keySelector.GetIdempotencyKey(context.RequestServices, controllerActionDescriptor, context);
        if (string.IsNullOrWhiteSpace(keyString))
        {
            if (idempotentAttribute.IsIdempotencyKeyRequired ?? options.IsIdempotencyKeyRequired)
                throw new ArgumentException("Controller action requires an idempotency key and none was supplied.");
            else
            {
                await _next(context);
                return;
            }
        }

        object key = options.KeyType switch
        {
            IdempotencyKeyType.NVarChar50 => keyString,
            IdempotencyKeyType.Guid => TryParseGuid(keyString),
            _ => throw new InvalidCastException($"KeyType {options.KeyType} unknown for idempotency.")
        };

        var databaseProvider = GetDatabaseProvider(options.ConnectionString ?? DefaultConnectionString);

        var keeperRequestContext = context.GetKeeperRequestContext();

        bool conflict;
        string? response;
        int? statusCode;

        (conflict, response, statusCode) =
            await databaseProvider.GetIdempotencyResult(tableName, key, idempotentAttribute.UseSqlTransaction, CancellationToken.None);

        if (conflict)
        {
            context.Response.ContentType = "application/json";
            if (response is not null)
            {
                context.Response.StatusCode = statusCode!.Value;
                await context.Response.WriteAsync(response);
                return;
            }
            else
            {
                context.Response.StatusCode = DefaultConflictResponseNotYetSavedStatusCode;
                await context.Response.WriteAsync(DefaultConflictResponseNotYetSavedResponse);
                return;
            }
        }

        if (keeperRequestContext != null)
        {
            keeperRequestContext.HttpContext = context;
            keeperRequestContext.SetIdempotencyKey(keyString);
        }

        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            response = await GetResponseBodyAsString(memoryStream);
            statusCode = context.Response.StatusCode;

            await databaseProvider.UpdateIdempotencyResponse(tableName, key, response, statusCode.Value, CancellationToken.None);

            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static async Task<string> GetResponseBodyAsString(MemoryStream memoryStream)
    {
        using var stream = new StreamReader(memoryStream, leaveOpen: true);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var text = await stream.ReadToEndAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        return text;
    }

    private static IDatabaseProvider GetDatabaseProvider(string connectionString) =>
        _memoryCache.GetOrCreate(connectionString, entry => connectionString.GetDatabaseProvider())!;

    private static Guid TryParseGuid(string keyString)
    {
        if (Guid.TryParse(keyString, out var guid))
            return guid;
        else
            throw new KeeperValidationException($"Idempotency key has to be a valid guid. value {keyString} is invalid.");
    }

    internal static void StepIntoForDebugger()
    {
        // Method intentionally left empty for debugging.
    }
}