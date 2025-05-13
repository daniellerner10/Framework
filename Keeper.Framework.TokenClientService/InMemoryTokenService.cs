using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;

namespace Keeper.Framework.TokenClientService;

internal class InMemoryTokenService : ITokenClientService
{
    private readonly IConfidentialClientApplication _msalClient;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _cacheExpirationBuffer = TimeSpan.FromMinutes(5);

    public InMemoryTokenService(
        IConfidentialClientApplication msalClient,
        IMemoryCache memoryCache)
    {
        _msalClient = msalClient ?? throw new ArgumentNullException(nameof(msalClient));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<string> GetAccessTokenAsync(string scope, CancellationToken cancellationToken)
    {
        // Create a cache key based on the scope
        string cacheKey = $"AccessToken_{scope}";

        // Get the token from cache
        var res = await _memoryCache.GetOrCreateAsync(cacheKey, async c =>
        {
            // If not in cache, acquire a new token
            var result = await _msalClient
                                .AcquireTokenForClient([scope])
                                .ExecuteAsync(cancellationToken);

            // Calculate an expiration time slightly before the actual token expiration
            // to avoid using tokens that are about to expire
            var expiresIn = result.ExpiresOn - DateTimeOffset.UtcNow - _cacheExpirationBuffer;
            // Only cache if the token won't expire immediately
            if (expiresIn > TimeSpan.Zero)
            {
                // Store the token in cache with the appropriate expiration
                c.AbsoluteExpirationRelativeToNow = expiresIn;
            }

            return result.AccessToken;
        });

        return res!;
    }
}