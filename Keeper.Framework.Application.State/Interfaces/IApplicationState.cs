namespace Keeper.Framework.Application.State;

/// <summary>
/// The keeper context for the request.
/// </summary>
public interface IApplicationState
{
    /// <summary>
    /// The correlation id for the request.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// The correlation id for the request.
    /// </summary>
    string IdempotencyKey { get; }

    internal void SetIdempotencyKey(string idempotencyKey);
    internal void SetCorrelationId(string correlationId);
}