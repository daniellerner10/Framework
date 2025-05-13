namespace Keeper.Framework.Middleware;

/// <summary>
/// The error response.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// List of all error details.
    /// </summary>
    public IList<ErrorDetail> Errors { get; private set; }

    /// <summary>
    /// Constructor for error response.
    /// </summary>
    /// <param name="error">The error detail.</param>
    public ErrorResponse(ErrorDetail error)
    {
        Errors = new List<ErrorDetail> { error };
    }

    /// <summary>
    /// Constructor for error response.
    /// </summary>
    /// <param name="errors">The error details.</param>
    public ErrorResponse(IList<ErrorDetail> errors)
    {
        Errors = errors;
    }
}
