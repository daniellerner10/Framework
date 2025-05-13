using Keeper.Framework.Middleware.WebApi.Contracts;
using Microsoft.AspNetCore.Builder;

namespace Keeper.Framework.Middleware;

public static class KeeperWebApiMiddlewareExtensions
{
    /// <summary>
    /// Use this to configure KeeperWebApiResponseMiddleware in aspnet .core Please use this in conjunction with
    /// <see cref="AddCustomKeeperValidation"/> in order to protect pipeline from illegal characters.
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <param name="configureOptions">configure the options</param>
    /// <returns>The application builder</returns>
    /// <example> 
    /// This sample shows how to configure <see cref="KeeperWebApiResponseMiddleware"/> in .net core WebApi
    /// <code>
    /// public class Startup
    /// {
    ///     public void ConfigureServices(IServiceCollection services)
    ///     {
    ///         services.AddCustomKeeperValidation();
    ///     }
    /// 
    ///     public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    ///     {
    ///         app.UseKeeperWebApiResponseMiddleware();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static WebApplication UseKeeperWebApiResponseMiddleware(
        this WebApplication builder,
        Action<KeeperWebApiResponseMiddlewareOptions> configureOptions)
    {
        ((IApplicationBuilder)builder).UseKeeperWebApiResponseMiddleware(configureOptions);

        return builder;
    }

    /// <summary>
    /// Use this to configure KeeperWebApiResponseMiddleware in aspnet .core Please use this in conjunction with
    /// <see cref="AddCustomKeeperValidation"/> in order to protect pipeline from illegal characters.
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <param name="configureOptions">configure the options</param>
    /// <returns>The application builder</returns>
    /// <example> 
    /// This sample shows how to configure <see cref="KeeperWebApiResponseMiddleware"/> in .net core WebApi
    /// <code>
    /// public class Startup
    /// {
    ///     public void ConfigureServices(IServiceCollection services)
    ///     {
    ///         services.AddCustomKeeperValidation();
    ///     }
    /// 
    ///     public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    ///     {
    ///         app.UseKeeperWebApiResponseMiddleware();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IApplicationBuilder UseKeeperWebApiResponseMiddleware(
        this IApplicationBuilder builder,
        Action<KeeperWebApiResponseMiddlewareOptions> configureOptions)
    {
        KeeperWebApiResponseMiddleware.StepIntoForDebugger();

        var options = new KeeperWebApiResponseMiddlewareOptions();
        configureOptions?.Invoke(options);

        KeeperWebApiResponseMiddleware.EnableUnifiedResponse = options.EnableUnifiedResponse;
        KeeperWebApiResponseMiddleware.CamelCaseUnifiedResponse = options.CamelCaseUnifiedResponse;
        KeeperWebApiResponseMiddleware.EnableExceptionHandling = options.EnableExceptionHandling;
        KeeperWebApiResponseMiddleware.LoggingPolicy = options.LoggingPolicy;
        KeeperWebApiResponseMiddleware.EnableLogContext = options.EnableLogContext;
        KeeperWebApiResponseMiddleware.EnableApplicationState = options.EnableApplicationState;
        KeeperWebApiResponseMiddleware.OnBeforeRequest = options.OnBeforeRequest;
        KeeperWebApiResponseMiddleware.OnAfterRequest = options.OnAfterRequest;
        KeeperWebApiResponseMiddleware.OnRequestError = options.OnRequestError;
        KeeperWebApiResponseMiddleware.ApiFilters = options.ApiFilters;

        var apiLogFilters = new ApiLogFilters
        {
            ApiLogRequestFilters = options.ApiLogRequestFilters,
            ApiLogResponseFilters = options.ApiLogResponseFilters,
            ApiLogRequestResponseFilters = options.ApiLogRequestResponseFilters,
        };

        builder.UseMiddleware<KeeperWebApiResponseMiddleware>(apiLogFilters);

        return builder;
    }
}
