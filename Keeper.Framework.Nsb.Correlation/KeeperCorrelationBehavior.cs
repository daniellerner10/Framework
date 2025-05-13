using Keeper.Framework.Application.State;
using NServiceBus.Pipeline;

namespace Keeper.Framework.Nsb.Correlation;

internal class KeeperCorrelationBehavior : Behavior<IIncomingPhysicalMessageContext>
{
    internal const string TraceMessageTemplate = "Keeper message trace.";

    public override async Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
    {
        var conversationId = GetConversationId(context);

        using (KeeperApplicationContext.PushState(conversationId, context.MessageId))
        {
            await next().ConfigureAwait(false);
        }
    }

    private static string GetConversationId(IIncomingPhysicalMessageContext context) =>
        context.MessageHeaders.ContainsKey(Headers.ConversationId) ? context.MessageHeaders[Headers.ConversationId] : string.Empty;
}