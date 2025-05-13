namespace Keeper.Framework.Application.State;

/// <summary>
/// Middleware context for sharing keeper request context between layers.
/// </summary>
public static class KeeperApplicationContext
{
    private static readonly AsyncLocal<IApplicationState> Data = new();

    private static IApplicationState CurrentApplicationState
    {
        get => Data.Value!;
        set => Data.Value = value;
    }

    /// <summary>
    /// Pushes application state with the provided correlation id and idempotency key.
    /// </summary>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <returns>A disposable object after context is complete.</returns>
    public static IDisposable PushState(string correlationId, string idempotencyKey)
    {
        IApplicationState applicationState = new ApplicationState();
        applicationState.SetCorrelationId(correlationId);
        applicationState.SetIdempotencyKey(idempotencyKey);

        return PushState(applicationState);
    }

    /// <summary>
    /// Pushes current keeper request context onto the stack.
    /// </summary>
    /// <param name="applicationState">The current keeper application state.</param>
    /// <returns>A disposable object after context is complete.</returns>
    public static IDisposable PushState(IApplicationState applicationState)
    {
        var bookmark = new ContextBookmark(CurrentApplicationState);

        CurrentApplicationState = applicationState;

        return bookmark;
    }

    /// <summary>
    /// Get current context.
    /// </summary>
    /// <returns>The current context.</returns>
    public static IApplicationState GetCurrentApplicationState() => CurrentApplicationState;

    private sealed class ContextBookmark : IDisposable
    {
        private readonly IApplicationState _bookmark;

        public ContextBookmark(IApplicationState bookmark)
        {
            _bookmark = bookmark;
        }

        public void Dispose()
        {
            CurrentApplicationState = _bookmark;
        }
    }
}