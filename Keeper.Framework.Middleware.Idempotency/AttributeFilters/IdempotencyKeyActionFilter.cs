using Keeper.Framework.Extensions.Http;
using Keeper.Framework.Validations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Keeper.Framework.Middleware.Idempotency;

public class IdempotencyKeyActionFilterAttribute : ActionFilterAttribute
{
    internal readonly static HashSet<string> _requiredIdempotencyKey = [HttpMethod.Post.ToString(), HttpMethod.Put.ToString()];

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (_requiredIdempotencyKey.Contains(context.HttpContext.Request.Method))
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(Headers.IdempotencyKey, out var idempotencyKeyValue))
                return;

            var idempotencyValue = idempotencyKeyValue.SingleOrDefault();
            if (string.IsNullOrWhiteSpace(idempotencyValue))
                return;

            if (!Guid.TryParse(idempotencyValue, out var idempotencyId))
                throw new KeeperValidationException($"{Headers.IdempotencyKey} header must be a valid Guid.");

            foreach (var param in context.ActionArguments)
                if (param.Value is BaseIdempotentRequestModel paramModel)
                    paramModel.IdempotencyKey = idempotencyId;
        }
    }
}