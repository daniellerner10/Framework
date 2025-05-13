namespace Keeper.Framework.Validations;

/// <summary>
/// Keeper validation exception.
/// </summary>
public class KeeperValidationException : Exception
{
    /// <summary>
    /// Validation issues that generated this exception.
    /// </summary>
    public IList<ValidationIssue> Issues { get; private set; }

    /// <summary>
    /// Constructor for Keeper validation exception.
    /// </summary>
    /// <param name="message">The message.</param>
    public KeeperValidationException(string message) : base(message)
    {
        Issues = new List<ValidationIssue> { new ValidationIssue(message) };
    }

    /// <summary>
    /// Constructor for Keeper validation exception.
    /// </summary>
    /// <param name="issues">The issues.</param>
    public KeeperValidationException(IList<ValidationIssue> issues)
    {
        Issues = issues;
    }
}