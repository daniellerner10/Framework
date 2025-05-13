using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Keeper.Framework.Middleware;

/// <summary>
/// Extensions for keeper request context.
/// </summary>
public static class KeeperRequestContextExtensions
{
    private const string KeeperRequestContext = "KeeperRequestContext";

    /// <summary>
    /// Add the keeper request context to the service collection.  Using this, you can
    /// inject <see cref="IKeeperRequestContext"/> into your controllers and get the same context
    /// that the KeeperWebApiResponseMiddleware" uses.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKeeperRequestContext(this IServiceCollection services)
    {
        services.AddScoped<IKeeperRequestContext, KeeperRequestContext>();

        return services;
    }

    /// <summary>
    /// Get keeper request context.
    /// </summary>
    /// <param name="context">the http context.</param>
    /// <returns>the keeper request context</returns>
    public static IKeeperRequestContext GetKeeperRequestContext(this HttpContext context)
    {
        var keeperRequestContext = context.RequestServices.GetService<IKeeperRequestContext>() ?? GetKeeperRequestWithoutServiceProvider(context);
        keeperRequestContext.SetContext(context);
        return keeperRequestContext;
    }

    private static IKeeperRequestContext GetKeeperRequestWithoutServiceProvider(HttpContext context)
    {
        if (!context.Items.ContainsKey(KeeperRequestContext))
            context.Items.Add(KeeperRequestContext, new KeeperRequestContext());

        return (IKeeperRequestContext)context.Items[KeeperRequestContext]!;
    }
}