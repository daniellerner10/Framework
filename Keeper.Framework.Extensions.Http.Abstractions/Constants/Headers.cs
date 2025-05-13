namespace Keeper.Framework.Extensions.Http;

/// <summary>
/// Header names to use with HttpClient.
/// </summary>
public static class Headers
{
    /// <summary>
    /// Idempotency key header name.
    /// </summary>
    public const string IdempotencyKey = "Idempotency-Key";
    /// <summary>
    /// Keeper correlation id header name.
    /// </summary>
    public const string KeeperCorrelationId = "Keeper-Correlation-Id";
}
