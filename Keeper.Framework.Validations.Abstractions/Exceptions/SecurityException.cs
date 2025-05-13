namespace Keeper.Framework.Validations;

/// <summary>
/// Security exception.
/// </summary>
public class SecurityException : Exception
{
    /// <summary>
    /// Constructor for security exception.
    /// </summary>
    /// <param name="message">The message.</param>
    public SecurityException(string message)
        : base(message)
    {
    }
}