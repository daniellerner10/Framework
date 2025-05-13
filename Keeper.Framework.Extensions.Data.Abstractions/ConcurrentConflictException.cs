namespace Keeper.Framework.Extensions.Data;

public class ConcurrentConflictException(string resourceName, string actionName, Exception? innerException = null)
  : Exception($"A concurrent {actionName} occurred for resource {resourceName}", innerException)
{
}
