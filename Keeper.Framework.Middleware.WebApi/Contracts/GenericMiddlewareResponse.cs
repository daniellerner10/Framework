namespace Keeper.Framework.Middleware;

internal class GenericMiddlewareResponse
{
    public int Status { get; set; }
    public string TraceId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Detail { get; set; } = default!;
    public Dictionary<string, List<string>> Errors { get; set; } = default!;
}