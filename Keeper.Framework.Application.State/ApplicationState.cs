namespace Keeper.Framework.Application.State;

public class ApplicationState : IApplicationState
{
    private Guid? _correlationId;
    private string _idempotencyKey = default!;

    /// <summary>
    /// The correlation id for the request.
    /// </summary>
    public Guid CorrelationId
    {
        get => _correlationId ??= Guid.NewGuid();
        init => _correlationId = value;
    }

    /// <summary>
    /// The correlation id for the request.
    /// </summary>
    public string IdempotencyKey
    {
        get => _idempotencyKey;
        init => _idempotencyKey = value;
    }

    void IApplicationState.SetIdempotencyKey(string idempotencyKey)
    {
        _idempotencyKey = idempotencyKey;
    }

    void IApplicationState.SetCorrelationId(string correlationId) => SetCorrelationId(correlationId);
    /// <summary>
    /// Set Correlation Id.
    /// </summary>
    /// <param name="correlationId">The correlation id.</param>
    protected void SetCorrelationId(string correlationId)
    {
        if (Guid.TryParse(correlationId, out var correlationGuid))
            _correlationId = correlationGuid;
    }
}
