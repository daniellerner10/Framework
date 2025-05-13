namespace Keeper.Framework.Nsb.Correlation;

/// <summary>
/// Extensions class for crb correlation.
/// </summary>
public static class CrbCorrelationExtensions
{
    /// <summary>
    /// Enable crb correlation feature.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    public static void EnableCrbCorrelation(this EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.EnableFeature<KeeperCorrelationFeature>();
    }

    /// <summary>
    /// Disable crb correlation feature.
    /// </summary>
    /// <param name="endpointConfiguration"></param>
    public static void DisableCrbCorrelation(this EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.DisableFeature<KeeperCorrelationFeature>();
    }
}