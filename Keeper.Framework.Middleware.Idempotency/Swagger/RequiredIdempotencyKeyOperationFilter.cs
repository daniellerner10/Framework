using Keeper.Framework.Extensions.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Keeper.Framework.Middleware.Idempotency;

public class RequiredIdempotencyKeyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IdempotencyKeyActionFilterAttribute._requiredIdempotencyKey.Contains(context.ApiDescription.HttpMethod!))
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new()
            {
                Required = true,
                Name = Headers.IdempotencyKey,
                In = ParameterLocation.Header
            });
        }
    }
}