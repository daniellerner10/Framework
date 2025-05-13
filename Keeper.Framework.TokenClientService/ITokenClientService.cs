namespace Keeper.Framework.TokenClientService;

public interface ITokenClientService
{
    Task<string> GetAccessTokenAsync(string scope, CancellationToken cancellationToken);
}
