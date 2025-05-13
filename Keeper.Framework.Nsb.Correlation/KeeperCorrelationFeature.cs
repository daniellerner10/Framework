using NServiceBus.Features;
using NServiceBus.Pipeline;

namespace Keeper.Framework.Nsb.Correlation;

public class KeeperCorrelationFeature : Feature
{
    /// <summary>
    /// Constructor for the Keeper correlation feature.
    /// </summary>
    public KeeperCorrelationFeature()
    {
        EnableByDefault();
    }

    /// <summary>
    /// Sets up the Keeper correlation feature.
    /// </summary>
    /// <param name="context">The feature configuration context.</param>
    protected override void Setup(FeatureConfigurationContext context)
    {
        var isSendOnly = context.Settings.GetOrDefault<bool>("Endpoint.SendOnly");

        if (isSendOnly)
            context.Pipeline.Register(new KeeperAddConversationIdStep());
        else
        {
            context.Pipeline.Register(new KeeperCorrelationStep());
        }
    }

    private sealed class KeeperCorrelationStep : RegisterStep
    {
        public KeeperCorrelationStep() : base(
            stepId: nameof(KeeperCorrelationBehavior),
            behavior: typeof(KeeperCorrelationBehavior),
            description: "Adds application state.")
        {
        }
    }

    private sealed class KeeperAddConversationIdStep : RegisterStep
    {
        public KeeperAddConversationIdStep() : base(
            stepId: nameof(KeeperAddConversationIdBehavior),
            behavior: typeof(KeeperAddConversationIdBehavior),
            description: "Adds conversation id from the Keeper correlation id as a conversation id.",
            factoryMethod: builder =>
            {
                return new KeeperAddConversationIdBehavior();
            })
        { }
    }
}