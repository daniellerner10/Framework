using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Keeper.Framework.Middleware.Idempotency;

/// <summary>
/// Inteface for a idempotency key selector.
/// </summary>
public interface IIdempotencyKeySelector
{
    /// <summary>
    /// Method which returns the idempotency key from the context.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="context">The http context.</param>
    /// <returns>The idempotency key.</returns>
    Task<string> GetIdempotencyKey(IServiceProvider serviceProvider, ControllerActionDescriptor controllerActionDescriptor, HttpContext context);
}