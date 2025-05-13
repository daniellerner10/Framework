
namespace Keeper.Framework.Validations;

/// <summary>
/// No content exception.
/// </summary>
public class NoContentException : Exception
{
    /// <summary>
    /// Constructor for no content exception.
    /// </summary>
    /// <param name="message">The message.</param>
    public NoContentException(string message)
        : base(message)
    {
    }
}
