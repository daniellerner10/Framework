using Keeper.Framework.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using System.Reflection;

namespace Keeper.Framework.Middleware.Idempotency;

internal class DefaultIdempotencyKeySelector : IIdempotencyKeySelector
{
    public async Task<string> GetIdempotencyKey(IServiceProvider serviceProvider, ControllerActionDescriptor controllerActionDescriptor, HttpContext context)
    {
        var key = context.Request.Headers[Headers.IdempotencyKey].SingleOrDefault();

        if (key is null)
        {
            var idempotentProp = controllerActionDescriptor
                .Parameters
                .SelectMany(x => x.ParameterType.GetProperties())
                .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(IdempotencyKeyAttribute)));

            if (idempotentProp is not null)
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                await using var jsonReader = new JsonTextReader(reader);

                while (await jsonReader.ReadAsync())
                    if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value?.ToString()?.Equals(idempotentProp.Name, StringComparison.InvariantCultureIgnoreCase) is true)
                    {
                        await jsonReader.ReadAsync();
                        key = jsonReader.Value?.ToString();
                        break;
                    }

                context.Request.Body.Position = 0;
            }
        }

        return key ?? string.Empty;
    }
}