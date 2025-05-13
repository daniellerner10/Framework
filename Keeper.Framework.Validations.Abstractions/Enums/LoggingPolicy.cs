namespace Keeper.Framework.Validations;

public enum LoggingPolicy
{
    /// <summary>
    /// No logging.
    /// </summary>
    None,
    /// <summary>
    /// Log only unhandled exceptions.
    /// </summary>
    UnhandledExceptionsOnly,
    /// <summary>
    /// Log, Before and after each request and log unhandled exceptions.
    /// </summary>
    All
}
