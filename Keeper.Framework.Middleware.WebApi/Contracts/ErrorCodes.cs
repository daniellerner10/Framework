namespace Keeper.Framework.Middleware;

/// <summary>
/// Error codes.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// A validation error.
    /// </summary>
    public const int Validation = 1000;
    /// <summary>
    /// A disclosure validation error.
    /// </summary>
    public const int DisclosureValidation = 1001;
    /// <summary>
    /// An application error.
    /// </summary>
    public const int Application = 2000;
    /// <summary>
    /// A security error.
    /// </summary>
    public const int Security = 3000;
    /// <summary>
    /// A system error.
    /// </summary>
    public const int System = 9999;
}