using System.Text.Json.Serialization;

namespace Keeper.Framework.Middleware.Idempotency;

public abstract class BaseIdempotentRequestModel
{
    [JsonIgnore]
    public Guid IdempotencyKey { get; set; } = Guid.Empty;
}