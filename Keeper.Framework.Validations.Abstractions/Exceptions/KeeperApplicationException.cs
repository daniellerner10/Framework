namespace Keeper.Framework.Validations;

public class KeeperApplicationException : Exception
{
    /// <summary>
    /// Constructor for keeper application exception.
    /// </summary>
    /// <param name="message">The message.</param>
    public KeeperApplicationException(string message)
        : base(message)
    {
    }
}