namespace Keeper.Framework.Validations;

/// <summary>
/// Conflict exception.
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Constructor for conflict exception.
    /// </summary>
    /// <param name="message">The message.</param>
    public ConflictException(string message)
        : base(message) { }

    /// <summary>
    /// Constructor for conflict exception.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}