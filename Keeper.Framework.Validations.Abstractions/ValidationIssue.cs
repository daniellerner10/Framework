namespace Keeper.Framework.Validations;

public class ValidationIssue
{
    /// <summary>
    /// The message.
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Constructor for a validation issue.
    /// </summary>
    /// <param name="message">The message.</param>
    public ValidationIssue(string message)
    {
        Message = message;
    }
}