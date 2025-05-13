using Keeper.Framework.Application.State;
using NServiceBus.Pipeline;

namespace Keeper.Framework.Nsb.Correlation;

internal class KeeperAddConversationIdBehavior : Behavior<IOutgoingLogicalMessageContext>
{
    public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
    {
        var applicationState = KeeperApplicationContext.GetCurrentApplicationState();
        if (applicationState != null)
        {
            if (context.Headers.ContainsKey(Headers.ConversationId))
                context.Headers[Headers.ConversationId] = applicationState.CorrelationId.ToString();
            else
                context.Headers.Add(Headers.ConversationId, applicationState.CorrelationId.ToString());
        }

        return next();
    }
}