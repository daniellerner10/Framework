namespace Keeper.Framework.Middleware;

/// <summary>
/// Model for error detail.
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// The error code.
    /// </summary>
    public int Code { get; set; }
    /// <summary>
    /// The error message.
    /// </summary>
    public string? Message { get; set; }
}