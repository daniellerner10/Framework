namespace Keeper.Framework.Validations;

/// <summary>
/// Not found exception.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Default constructor for not found exception.
    /// </summary>
    public NotFoundException()
        : base("Resource not found")
    {

    }

    /// <summary>
    /// Constructor for not found exception.
    /// </summary>
    /// <param name="message">The message.</param>
    public NotFoundException(string message)
        : base(message)
    {

    }
}

