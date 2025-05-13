using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Keeper.Framework.TokenClientService;

public static class TokenClientServiceExtensions
{
    public static IServiceCollection AddTokenClientService(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(TokenClientServiceOptions));
        services.Configure<TokenClientServiceOptions>(section);
        return services.AddTokenClientServiceIntertnal();
    }

    public static IServiceCollection AddTokenClientService(this IServiceCollection services, Action<TokenClientServiceOptions> configure)
    {
        services.Configure(configure);
        return services.AddTokenClientServiceIntertnal(); ;
    }

    private static IServiceCollection AddTokenClientServiceIntertnal(this IServiceCollection services)
    {
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<TokenClientServiceOptions>>().Value;
        services.AddMemoryCache();

        services.AddSingleton<ITokenClientService, InMemoryTokenService>();
        services.AddSingleton<IConfidentialClientApplication>(sp =>
        {
            // Build the MSAL confidential client
            var msalClient = ConfidentialClientApplicationBuilder
                .Create(options.ClientId)
                .WithAuthority(options.Authority)
                .WithClientSecret(options.ClientSecret)
                .Build();

            return msalClient;
        });

        return services;
    }
}
