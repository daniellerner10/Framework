namespace Keeper.Framework.Extensions.Http;

/// <summary>
/// Keeper Http Request Exception
/// </summary>
public class KeeperHttpRequestException : Exception
{
    /// <summary>
    /// A constructor.
    /// </summary>
    public KeeperHttpRequestException() : base() { }

    /// <summary>
    /// A constructor.
    /// </summary>
    /// <param name="message">The message.</param>
    public KeeperHttpRequestException(string message) : base(message) { }

    /// <summary>
    /// A constructor.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public KeeperHttpRequestException(string message, Exception innerException) : base(message, innerException) { }
}

