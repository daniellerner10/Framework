using System.Collections.Concurrent;

namespace Keeper.Framework.Middleware.Idempotency;

internal class IdempotencyTables : ConcurrentDictionary<string, IdempotencyTableOptions>
{
}